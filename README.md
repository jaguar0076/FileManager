FileManager
===========

File Manager project

Ce projet a pour but de créer un gestionnaire de fichiers intelligent pour les fichiers audio/vidéos, l'idée ici est de pouvoir gérer automatiquement et intelligemment les fichiers pour créer des dossiers appropriés.

Selon les Meta tags des fichiers il sera possible de créer un dossier pour trier les musiques/vidéos par artiste/album/année/etc....

Ce projet se présentera sous forme d'un service (forme finale, lors des tests le service sera remplacé par une application fenêtrée) qui sera capable d'analyser une arborescence de fichiers, stocker cette arborescence (le format XML est envisagé ici pour stocker cette arborescence et les attributs des fichiers), par la suite un "watcher" sera mis en place pour surveiller l'activité des fichiers, trier et classer les fichiers si nécessaire et modifier l'arborescence stockée.

Le stockage de l'arborescence en XML permettra d'éviter l'analyse complète de l'arborescence de fichier à chaque fois (action fort consommatrice de ressource, tant au point vue CPU (à optimiser le plus possible) et HDD (qui travaille énormément)).

Toute idée/aide/collaboration est la bienvenue :) N'étant pas un développeur C# chevronné :)
