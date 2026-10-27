# PodsDesk 1.0

Compagnon Windows créé pour Dyskolos_ — https://dyskolos.fr.

## Installation

PodsDesk-Setup-1.0.exe installe l’application pour le compte Windows actuel, sans pilote ni droits administrateur. Il ajoute un raccourci au menu Démarrer et, au choix, sur le bureau. Windows 10/11 x64 et le .NET Desktop Runtime 10 x64 sont nécessaires. Si le runtime est absent, l’installateur propose la page officielle de Microsoft. Le programme n’est pas signé numériquement : Windows peut afficher un avertissement SmartScreen.

Fermez les anciennes instances de PodsDesk depuis la zone de notification avant de démarrer la version installée. Après fermeture de la fenêtre, l’application reste dans la zone de notification ; double-cliquez sur son icône pour la rouvrir, ou cliquez avec le bouton droit puis Quitter pour l’arrêter. La désinstallation est accessible dans Paramètres Windows > Applications ; les préférences restent dans %LOCALAPPDATA%\PodsDesk.

## Fonctions disponibles

- Interface claire et anthracite, animations, cinq langues et reconnaissance d’un AirPods connecté.
- Batteries gauche, droite et boîtier, niveau dans la zone de notification, état de charge lorsque diffusé et heure de mise à jour.
- Alertes configurables, lancement facultatif avec Windows, historique de session effaçable et export CSV à la demande.
- Commandes multimédias de Windows, paramètres Bluetooth et sélection manuelle d’un signal si nécessaire.

## Limites

L’association automatique est une estimation fondée sur le modèle et la puissance du signal, pas une preuve d’identité. Un autre AirPods à proximité peut être choisi. Les niveaux sont diffusés par paliers de 10 % et les informations peuvent manquer. La connexion est détectée pour les périphériques Bluetooth classiques dont le nom contient « AirPods ». La compatibilité varie suivant les modèles et adaptateurs ; le projet n’a pas été validé avec tous les appareils.

La réduction de bruit, la transparence, la batterie à 1 % près et les autres fonctions nécessitant le protocole privé ne sont pas disponibles dans cette version. Aucune affiliation à Apple.

Le décodage s’appuie sur les recherches communautaires : https://github.com/ahmadashwah/AirPodsTray/blob/main/airpods_scanner.py et https://github.com/fischejo/airpods-notify/blob/master/doc/proximity_protocol.md.
