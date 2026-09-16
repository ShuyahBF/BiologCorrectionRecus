using System;
using System.Collections.Generic;
using System.IO;

namespace BiologCorrectionRecus.Configuration
{
    /// <summary>
    /// Lit un fichier d'initialisation (.ini) au format classique Windows :
    /// <code>
    /// [Section]
    /// Cle=Valeur
    /// </code>
    ///
    /// Chaque logiciel (Aizenta, Biolog, eKol, ...) a son propre fichier .ini dans lequel un
    /// administrateur configure les ressources réelles du poste (ex : nom de la base HFSQL,
    /// section "Serveur", clé "Nom"). Ce lecteur est volontairement générique (indépendant de
    /// Biolog) pour être réutilisé tel quel sur tous les projets qui suivent cette même logique.
    /// </summary>
    internal static class IniFileReader
    {
        /// <summary>
        /// Charge tout le contenu d'un fichier .ini en mémoire : section -> (clé -> valeur).
        /// Les noms de section et de clé sont insensibles à la casse (comme sous Windows).
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> Lire(string cheminFichier)
        {
            var resultat = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string sectionCourante = string.Empty;

            foreach (string ligneBrute in File.ReadAllLines(cheminFichier))
            {
                string ligne = ligneBrute.Trim();

                // Ligne vide ou commentaire (";" et "#" sont les deux styles courants) : ignorée.
                if (ligne.Length == 0 || ligne.StartsWith(';') || ligne.StartsWith('#'))
                    continue;

                // Nouvelle section : "[NomDeSection]"
                if (ligne.StartsWith('[') && ligne.EndsWith(']'))
                {
                    sectionCourante = ligne[1..^1].Trim();
                    if (!resultat.ContainsKey(sectionCourante))
                        resultat[sectionCourante] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                // Ligne "Cle=Valeur"
                int indexEgal = ligne.IndexOf('=');
                if (indexEgal <= 0)
                    continue; // ligne mal formée (pas de "="), on l'ignore plutôt que de planter

                string cle = ligne[..indexEgal].Trim();
                string valeur = ligne[(indexEgal + 1)..].Trim();

                if (!resultat.ContainsKey(sectionCourante))
                    resultat[sectionCourante] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                resultat[sectionCourante][cle] = valeur;
            }

            return resultat;
        }

        /// <summary>Raccourci pour lire une seule valeur ([Section] Cle=Valeur), ou null si absente.</summary>
        public static string? LireValeur(string cheminFichier, string section, string cle)
        {
            var contenu = Lire(cheminFichier);
            return contenu.TryGetValue(section, out var clesDeLaSection) && clesDeLaSection.TryGetValue(cle, out var valeur)
                ? valeur
                : null;
        }
    }
}
