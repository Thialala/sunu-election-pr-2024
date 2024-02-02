# Traitement automatisé des Bureaux de Vote - Élections Présidentielles Sénégal 2024

Ce projet vise à traiter et organiser les informations relatives aux bureaux de vote du Sénégal pour les élections présidentielles de 2024. Le processus est entièrement automatisé, depuis l'extraction des données à partir de fichiers PDF jusqu'à la génération de fichiers CSV consolidés par région, département et commune.
[CARTE_ELECTORALE_ELECTION_PR_DU_25FEV2024.csv](CARTE_ELECTORALE_ELECTION_PR_DU_25FEV2024.csv)


## Processus de Traitement

Le traitement s'effectue en plusieurs étapes clés :

1. **Extraction PDF :** Chaque page du fichier PDF original, contenant les informations des bureaux de vote, est exportée en un fichier PDF distinct.
2. **OCR et Conversion en Markdown :** Les fichiers PDF sont ensuite traités par OCR (Reconnaissance Optique de Caractères) pour convertir les données textuelles en fichiers Markdown, facilitant l'extraction des tables de données.
3. **Extraction des Données et Création des CSV :** Les fichiers Markdown sont lus un par un, et les données des tables sont extraites pour créer un fichier CSV associé pour chaque fichier Markdown.
4. **Regroupement des CSV :** Tous les fichiers CSV sont finalement regroupés en un seul fichier consolidé, fournissant une vue d'ensemble des bureaux de vote sur l'ensemble du territoire sénégalais.

## Dossier de Sortie

Les fichiers PDF traités, les fichiers Markdown générés par OCR et les fichiers CSV correspondants sont tous stockés dans le dossier `données`. Pour chaque région, département et commune, vous trouverez un ensemble de fichiers PDF et CSV associés.

Il est **important** de consulter régulièrement le dossier `données` pour vérifier les données et s'assurer de leur exactitude.

## Rapport d'erreurs

Le processus étant 100% automatique, des erreurs peuvent survenir lors de l'extraction ou de la conversion des données. Si vous identifiez des incohérences ou des erreurs dans les fichiers générés, merci de les remonter pour correction. Votre feedback est crucial pour améliorer la qualité et l'exactitude des informations.

## Contribution et Corrections

Pour contribuer au projet ou signaler des erreurs, veuillez suivre ces étapes :

1. **Identification de l'erreur :** Notez le nom du fichier et la nature de l'erreur.
2. **Signalement :** Envoyez un rapport d'erreur détaillé, incluant le nom du fichier et une description précise de l'erreur, à l'adresse email fournie ou via le système de gestion des issues de ce projet.
3. **Correction :** Les corrections seront apportées dès que possible, et les fichiers mis à jour seront réintégrés dans le dossier `données`.

Votre collaboration assure la fiabilité et la précision des données fournies pour les élections présidentielles du Sénégal en 2024.