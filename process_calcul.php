<?php
// Désactiver l'affichage des erreurs pour éviter d'interférer avec la réponse JSON
ini_set('display_errors', 0);
error_reporting(0);

// Définir le type de contenu comme JSON
header('Content-Type: application/json');

// Récupérer les données POST envoyées en JSON
$input = json_decode(file_get_contents('php://input'), true);

if (isset($input['calcul'])) {
    $calcul = $input['calcul'];
    $pythonPath = 'python';
    $scriptPath = 'script.py';
    $command = escapeshellcmd("$pythonPath $scriptPath " . escapeshellarg($calcul)) . ' 2>&1';

    $output = shell_exec($command);

    if ($output === null) {
        echo json_encode(['error' => 'Erreur lors de l\'exécution du script Python']);
        exit;
    }
    $output = trim($output);

    $data = json_decode($output, true);

    if (json_last_error() !== JSON_ERROR_NONE) {
        echo json_encode(['error' => 'Réponse invalide du script Python']);
        exit;
    }

    if (isset($data['error'])) {
        echo json_encode(['error' => $data['error']]);
    } else {
        echo json_encode(['resultat' => $data['resultat']]);
    }
} else {
    echo json_encode(['error' => 'Aucun calcul fourni']);
}
?>
