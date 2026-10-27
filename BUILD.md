# Construire PodsDesk 1.0

Sous Windows x64, installer le SDK .NET 10 et .NET Framework 4.8, puis exécuter `powershell -ExecutionPolicy Bypass -File .\Build-Installer.ps1`. Le résultat est `artifacts\PodsDesk-Setup-1.0.exe`. Le scanner Windows Runtime précompilé est fourni ; sa source figure dans `PodsDesk\Scanner.cs.txt`.

L’installateur construit est non signé. Il dépend du .NET Desktop Runtime 10 x64, n’installe pas de pilote et ne fournit pas les commandes de contrôle du bruit. Les sources du programme d’installation sont dans `installer\Setup.cs` ; les paramètres de l’utilisateur sont conservés à la désinstallation.
