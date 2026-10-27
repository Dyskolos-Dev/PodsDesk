# PodsDesk

Compagnon AirPods pour Windows, créé pour **Dyskolos_** — [dyskolos.fr](https://dyskolos.fr).

PodsDesk affiche les niveaux de batterie reçus par Bluetooth, propose une icône de notification, un historique de session, des alertes configurables et des commandes multimédias Windows. L’interface est disponible en français, anglais, espagnol, allemand et portugais, avec thèmes clair et sombre.

## Installation

Windows 10/11 x64 et le **.NET Desktop Runtime 10 x64** sont nécessaires. L’installateur `PodsDesk-Setup-1.0.exe` est prévu dans les releases du projet. Il installe l’application pour le compte Windows actuel. L’exécutable n’est pas signé ; Windows peut afficher un avertissement SmartScreen.

Fermez les anciennes instances de PodsDesk depuis la zone de notification avant de lancer la version installée. La fermeture de la fenêtre réduit l’application dans cette zone ; clic droit sur son icône, puis Quitter, pour l’arrêter.

## Limites actuelles

La batterie est estimée par paliers de 10 %. L’association automatique s’appuie sur le modèle et la puissance du signal Bluetooth : elle peut confondre une paire voisine. L’état de charge et certains niveaux ne sont pas toujours diffusés. La connexion est détectée pour les appareils Bluetooth classiques dont le nom contient « AirPods ».

La réduction du bruit, la transparence, la batterie au pourcentage près et les autres fonctionnalités qui nécessitent le protocole privé des AirPods **ne sont pas encore disponibles**. Le code expérimental qui prépare les commandes de contrôle du bruit ne les transmet pas aux écouteurs. Aucun pilote n’est fourni ou installé.

## Développement

Les sources de l’application sont dans `PodsDesk/`, celles de l’installateur dans `installer/`. Consultez [BUILD.md](BUILD.md) pour construire l’application et l’installateur. Le scanner compilé est fourni avec sa source `PodsDesk/Scanner.cs.txt`.

## Crédits

Projet créé pour Dyskolos_. Non affilié à Apple. Les informations Bluetooth s’appuient notamment sur les travaux communautaires de [AirPodsTray](https://github.com/ahmadashwah/AirPodsTray/blob/main/airpods_scanner.py), [airpods-notify](https://github.com/fischejo/airpods-notify/blob/master/doc/proximity_protocol.md) et [LibrePods](https://github.com/librepods-org/librepods/blob/main/docs/control_commands.md).
