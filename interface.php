<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Calculatrice Python</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f0f0f0;
            margin: 0;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }

        .container {
            background-color: #fff;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            width: 300px;
            text-align: center;
        }

        h1 {
            font-size: 24px;
            margin-bottom: 20px;
            color: #333;
        }

        input[type="text"] {
            width: 100%;
            padding: 10px;
            font-size: 16px;
            border: 1px solid #ccc;
            border-radius: 5px;
            box-sizing: border-box;
            margin-bottom: 20px;
        }

        button {
            background-color: #28a745;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            transition: background-color 0.3s;
        }

        button:hover {
            background-color: #218838;
        }

        #resultat {
            margin-top: 20px;
            font-size: 18px;
            color: #555;
        }

        /* Responsive */
        @media (max-width: 400px) {
            .container {
                width: 90%;
            }

            h1 {
                font-size: 20px;
            }

            input[type="text"], button {
                font-size: 14px;
            }
        }
    </style>
    <script>
        function envoyerCalcul() {
            const calcul = document.getElementById("calcul").value;

            fetch('process_calcul.php', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ calcul: calcul })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Erreur réseau');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.resultat) {
                        document.getElementById("resultat").innerHTML = data.resultat;
                    } else if (data.error) {
                        document.getElementById("resultat").innerHTML = "Erreur: " + data.error;
                    }
                })
                .catch(error => {
                    document.getElementById("resultat").innerHTML = "Erreur: " + error.message;
                    console.error('Erreur:', error);
                });
        }
    </script>
</head>
<body>
<div class="container">
    <h1>Calculatrice Python</h1>
    <input type="text" id="calcul" placeholder="Entrez votre calcul" />
    <button onclick="envoyerCalcul()">Calculer</button>
    <p id="resultat"></p>
</div>
</body>
</html>
