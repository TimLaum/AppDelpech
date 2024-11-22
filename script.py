import sys
import requests
import json
import logging

# Configurer le logger pour écrire dans un fichier
logging.basicConfig(filename='debug.log', level=logging.DEBUG, format='%(asctime)s - %(message)s')

def verifier_calcul_deja_fait(calcul):
    url = "http://localhost:5001/"
    try:
        response = requests.get(url)
        if response.status_code == 200:
            calculs = response.json()
            for c in calculs:
                if c['Calcul'] == calcul:
                    return c['Resultat']
    except requests.exceptions.RequestException as e:
        logging.error(f"Erreur lors de la vérification du calcul: {str(e)}")
        return {"error": f"Erreur lors de la vérification du calcul: {str(e)}"}
    return None

# Fonction pour vérifier si un nombre est premier
def est_premier(n):
    if n < 2:
        return False
    for i in range(2, int(n**0.5) + 1):
        if n % i == 0:
            return False
    return True

# Fonction pour vérifier si un nombre est parfait
def est_parfait(n):
    if n < 2:
        return False
    somme_diviseurs = sum(i for i in range(1, n) if n % i == 0)
    return somme_diviseurs == n

# Fonction pour effectuer le calcul
def calculer(expression):
    try:
        # Calcul du résultat de l'expression
        resultat = eval(expression)

        # Déterminer les propriétés du résultat
        est_pair = resultat % 2 == 0
        est_prem = est_premier(resultat)
        est_parf = est_parfait(resultat)

        # Loguer les détails du calcul
        logging.debug(f"Calcul: {expression} -> Résultat: {resultat}, Parité: {est_pair}, Premier: {est_prem}, Parfait: {est_parf}")

        return {
            "resultat": resultat,
            "est_pair": est_pair,
            "est_premier": est_prem,
            "est_parfait": est_parf,
            "calcul": expression,
        }
    except Exception as e:
        logging.error(f"Erreur lors du calcul de l'expression: {expression}. Erreur: {str(e)}")
        return {"error": str(e)}

if __name__ == "__main__":
    if len(sys.argv) < 2:
        logging.error("Aucun calcul fourni.")
        print(json.dumps({"error": "Aucun calcul fourni"}))
        sys.exit(1)

    calcul = sys.argv[1]
    resultat_existant = verifier_calcul_deja_fait(calcul)
    if isinstance(resultat_existant, dict) and "error" in resultat_existant:
        logging.error(resultat_existant["error"])
        print(json.dumps(resultat_existant))
    elif resultat_existant:
        logging.info(f"Calcul déjà effectué. Résultat : {resultat_existant}")
        print(json.dumps({"resultat": f"Calcul déjà effectué. Résultat : {resultat_existant}"}))
    else:
        resultat = calculer(calcul)
        if "error" in resultat:
            logging.error(resultat["error"])
            print(json.dumps(resultat))
        else:
            data = {
                "calcul": calcul,
                "resultat": str(resultat["resultat"]),
                "est_pair": resultat["est_pair"],
                "est_premier": resultat["est_premier"],
                "est_parfait": resultat["est_parfait"]
            }
            url = "http://localhost:5001/api/calcul"
            try:
                response = requests.post(url, json=data)
                logging.info(f"Réponse de l'API: {response.text}")
                print(json.dumps({"resultat": response.text}))
            except requests.exceptions.RequestException as e:
                logging.error(f"Erreur lors de l'appel à l'API C#: {str(e)}")
                print(json.dumps({"error": f"Erreur lors de l'appel à l'API C#: {str(e)}"}))
