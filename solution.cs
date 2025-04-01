/* TODO:
 * 
 * DONE:
 * Prvi primjer dobro radi.
 * Drugi primjer dobro radi.
 * Treci primjer dobro radi.
 * Cetvrti primjer dobro radi.
 */

using System.Text.Json.Nodes;

internal class Program
{
    private static void Main(string[] argv)
    {
        if (argv.Length != 1) throw new Exception("Greska! Broj komandnolinijskih argumenata mora biti jednak 1.");
        Ispis i = new();
        i.IspisiDatoteku(argv[0]);
    }
}

class Ispis
{
#pragma warning disable IDE0044 // Add readonly modifier
    HashSet<string> visited; // Skup vec posjecenih referenci. Sprjecava cikluse
#pragma warning restore IDE0044 // Add readonly modifier

    public Ispis() { visited = new(); }

    // "main" funkcija
    public void IspisiDatoteku(string fileName)
    {
        JsonNode node;

        using (StreamReader r = new(fileName))
        {
            string jsonString = r.ReadToEnd();
            node = JsonNode.Parse(jsonString)!;
        }

        string output = "";

        output += "{\"ime\":\"";
        output += node["ime"]!.ToString();
        output += "\"";

        JsonNode root = node.Root;
        if (root["sastojci"] is JsonNode sastojci) { output += ","; output += IspisiSastojke(sastojci); }
        if (root["koraci"] is JsonNode koraci) { output += IspisiKorake(koraci); }
        if (root["posluzivanje"] is JsonNode upute) { output += ","; output += IspisiUpute(upute); }

        output += "}";

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        /*string expectedOutput1 = "{\"ime\":\"Slatka sol\",\"sastojci\":[{\"ime\":\"secer\",\"kolicina\":100},{\"ime\":\"sol\",\"kolicina\":1000},{\"ime\":\"Led (ovo je primjer grupe sastojaka)\",\"sastojci\":[{\"ime\":\"usitnjen led\",\"kolicina\":100},{\"ime\":\"kockice leda\",\"kolicina\":100}]}],\"koraci\":[\"Pripremite dvije posude\",{\"ime\":\"Vaganje\",\"koraci\":[\"Izvazite secer\",\"Izvazite sol\"]},\"Pomijesajte sadrzaj posuda\",\"Dodajte jos secera po ukusu\"],\"posluzivanje\":[\"Ukrasite ledom\"]}";
        string expectedOutput2 = "{\"ime\":\"Recept A\",\"sastojci\":[{\"ime\":\"Sastojak koji ce se ponoviti\",\"kolicina\":1},{\"ime\":\"X\",\"sastojci\":[{\"ime\":\"Sastojak koji ce se ponoviti\",\"kolicina\":1}]},{\"ime\":\"Y\",\"sastojci\":[{\"ime\":\"Sastojak koji ce se ponoviti\",\"kolicina\":1}]},{\"sastojci\":[{\"ime\":\"Sastojak koji ce se ponoviti\",\"kolicina\":1}]}]}";
        string expectedOutput3 = "{\"ime\":\"Recept A\",\"koraci\":[\"Korak 1\",\"Korak 2\",\"Korak 2\",\"Korak 1\"]}";
        string expectedOutput4 = "{\"ime\":\"Recept C\",\"sastojci\":[{\"ime\":\"Sastojak\",\"kolicina\":314},{\"ime\":\"X\",\"sastojci\":[{\"ime\":\"Sastojak\",\"kolicina\":314}]}]}";
        string expectedOutputMin = "{\"ime\":\"Prazan recept\"}";*/
#pragma warning restore CS0219 // Variable is assigned but its value is never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0079 // Remove unnecessary suppression

        /*string expectedOutput = expectedOutput4;

        if (output == expectedOutput) Console.WriteLine("IZLAZ JE JEDNAK OCEKIVANOM IZLAZU");
        Console.WriteLine();*/
        Console.WriteLine(output);
        /*Console.WriteLine();
        Console.WriteLine(expectedOutput);*/
    }

    // Ova funkcija za zadanu listu (sastojaka, koraka) listRoot trazi rjecnik imena dictionaryName
    JsonNode PronadjiIzListe(JsonNode listRoot, string dictionaryName)
    {
        JsonArray listElements = listRoot.AsArray();
        int length = listElements.Count;
        for (int i = 0; i < length; i++)
        {
            JsonNode node = PronadjiIzKorijena(listElements[i]!, dictionaryName);
            if (node != null)
            {
                return node; // Pronasli smo ga
            }
        }

        // Nismo pronasli rjecnik u ovoj listi
#pragma warning disable CS8603 // Possible null reference return.
        return null;
    }

    // Ova funkcija za zadani korijen root trazi rjecnik imena dictionaryName
    JsonNode PronadjiIzKorijena(JsonNode root, string dictionaryName)
    {
        JsonNode node; // node kojeg cemo vratiti na kraju

        if (root["ime"] is JsonNode ime)
        {
            if (ime.ToString() == dictionaryName)
            {
                return root;
            }

            // Uvjet gornjeg if-a nije ispunjen, dakle trazeni rjecnik ne nalazi se u korijenu
            // Probajmo ga sada pronaci u listi sastojaka:
            // Sadrzi li uopce dani JSON sastojke ili samo korake, kao u primjeru3?
            if (root["sastojci"] is JsonNode listaSastojaka) // Ako sadrzi sastojke, potrazimo ga u njima
            {
                node = PronadjiIzListe(listaSastojaka, dictionaryName);
                if (node != null)
                {
                    return node; // Pronasli smo ga
                }
            }

            // Lista sastojaka ili ne postoji ili ne sadrzi trazeni rjecnik, probajmo ga pronaci u listi koraka, ako postoji:
            if (root["koraci"] is JsonNode listaKoraka) // Ako sadrzi korake, potrazimo ga u njima
            {
                node = PronadjiIzListe(listaKoraka, dictionaryName);
                if (node != null)
                {
                    return node; // Pronasli smo ga
                }
            }

            // Lista koraka ili ne postoji ili ne sadrzi trazeni rjecnik, probajmo ga pronaci u posluzivanju:
            if (root["posluzivanje"] is JsonNode listaUputa) // Ako sadrzi korake, potrazimo ga u njima
            {
                node = PronadjiIzListe(listaUputa, dictionaryName);
                if (node != null) return node; // Pronasli smo ga
            }

            // Nismo ga pronasli u (pod)stablu s ovim korijenom
        }
        else if (root["sastojci"] is JsonNode listaSastojaka)
        {
            node = PronadjiIzListe(listaSastojaka, dictionaryName);
            if (node != null)
            {
                return node; // Pronasli smo ga
            }
        }
        else if (root["ref"] is JsonNode referenca)
        {
            if (visited.Contains(referenca.ToString())) return null;

            visited.Add(root["ref"]!.ToString());

            string path = referenca.ToString();
            string fileName = path[..path.IndexOf('#')];

            JsonNode newRoot; // Dohvatit cemo korijen json dokumenta s imenom fileName
            using (StreamReader r = new(fileName))
            {
                string jsonString = r.ReadToEnd();
                newRoot = JsonNode.Parse(jsonString)!.Root;
            }

            /*int indexLimit;
            int lastDetectedSlash = path.IndexOf('/');
            if (lastDetectedSlash == -1)
            {
                indexLimit = path.Length;
            }
            else { indexLimit = lastDetectedSlash; }*/
            
            return PronadjiIzKorijena(newRoot, dictionaryName);
        }

        // Mislim da se ovaj return ne bi trebao nikada dogoditi
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }

    string IspisiSastojak(JsonNode sastojakNode)
    {
        string output = "";

        if (sastojakNode["ime"] is JsonNode ime)
        {
            output += "{\"ime\":\"";
            output += ime.ToString();
            output += "\",";
        }
        else { output += "{"; }

        if (sastojakNode["sastojci"] is JsonNode sastojci) { output += IspisiSastojke(sastojci); }
        else if (sastojakNode["kolicina"] is not null)
        {
            output += "\"kolicina\":";
            output += sastojakNode["kolicina"]!.ToString();
        }
        else { throw new Exception("\nNeocekivana struktura JSON dokumenta!"); }

        output += "}";

        return output;
    }
    string IspisiSastojke(JsonNode sastojciNode)
    {
        string output = "";

        /*if (sastojciNode["ref"] is JsonObject)
        {
            string path = sastojciNode["ref"]!.ToString();
            Console.WriteLine("krivo");
        }*/

        output += "\"sastojci\":[";
        JsonArray sastojci = sastojciNode.AsArray();
        int length = sastojci.Count;
        for (int i = 0; i < length; i++)
        {
            JsonNode node = sastojci[i]!; // Sastojak kojeg cemo ispisati, bio on referenca ili ne

            // Ako je sastojak zapravo referenca, imamo posebnu proceduru:
            while (node["ref"] is JsonNode referenca)
            {
                string path = referenca.ToString();
                string fileName = path[..path.IndexOf('#')];

                JsonNode root; // Dohvatit cemo korijen json dokumenta s imenom fileName
                using (StreamReader r = new(fileName))
                {
                    string jsonString = r.ReadToEnd();
                    root = JsonNode.Parse(jsonString)!.Root;
                }

                // Kroz putanju cemo prolaziti dio po dio, odnosno od slasha do slasha
                // Varijabla indexLimit oznacavat ce desnu granicu dijela kojeg trenutno istrazujemo
                int indexLimit;
                int lastDetectedSlash = path.IndexOf('/');
                if (lastDetectedSlash == -1) // Referenca ne sadrzi slash, dakle sastoji se samo od imena datoteke, simbola # i imena rjecnika
                {
                    indexLimit = path.Length;
                }
                else { indexLimit = lastDetectedSlash; }
                string dictionaryName = path[(path.IndexOf('#') + 1)..indexLimit];
                // U json dokumentu s danim imenom fileName potrazit cemo odgovarajuci rjecnik s imenom dictionaryName
                visited.Add(referenca.ToString());
                node = PronadjiIzKorijena(root, dictionaryName);

                if (lastDetectedSlash != -1) // Referenca je sadrzavala slash, odnosno nije se sastojala samo od imena datoteke, simbola # i imena rjecnika
                {
                    while (lastDetectedSlash != path.LastIndexOf('/')) // Sve dok nismo procitali zadnji slash u pathu...
                    {
                        int nextSlash = path.IndexOf('/', lastDetectedSlash + 1);
                        string dictionaryKey = path[(lastDetectedSlash + 1)..nextSlash]; // Citamo dio patha izmedju zadnjeg otkrivenog slasha i iduceg neotkrivenog slasha

                        JsonArray listElements = node[dictionaryKey]!.AsArray(); // Dohvatili smo listu koja odgovara kljucu dictionaryKey (npr "sastojci" u pathu "a.json#X/sastojci/1")

                        lastDetectedSlash = nextSlash;
                        nextSlash = path.IndexOf('/', lastDetectedSlash + 1);
                        if (nextSlash == -1) nextSlash = path.Length;
                        int index = Convert.ToInt32(path[(lastDetectedSlash + 1)..nextSlash]); // Indeks elementa liste

                        node = listElements[index - 1]!; // - 1 jer kuharovo indeksiranje krece jedinicom

#pragma warning disable IDE0150 // Prefer 'null' check over type check
                        if (node["ref"] is JsonNode) continue;
#pragma warning restore IDE0150 // Prefer 'null' check over type check
                        else
                        {
                            visited!.Clear();
                            break;
                        }
                    }
                }
            }

            output += IspisiSastojak(node);
            if (i != length - 1) output += ",";
        }
        output += "]";

        return output;
    }

    string IspisiKorak(JsonNode korakNode)
    {
        string output = "";

        if (korakNode["ime"] is JsonNode ime)
        {
            output += "{\"ime\":\"";
            output += ime.ToString();
            output += "\"";
        }
        else { output += "{"; }
        if (korakNode["koraci"] is JsonNode koraci) { output += IspisiKorake(koraci); }
        else { throw new Exception("\nNeocekivana struktura JSON dokumenta!"); }

        output += "}";

        return output;
    }
    string IspisiKorake(JsonNode koraciNode)
    {
        string output = "";

        output += ",\"koraci\":[";
        JsonArray koraci = koraciNode.AsArray();
        int length = koraci.Count;
        for (int i = 0; i < length; i++)
        {
            if (koraci[i] is JsonObject)
            {
#pragma warning disable IDE0150 // Prefer 'null' check over type check
                if (koraci[i]!["ime"] is JsonNode)
                {
                    output += IspisiKorak(koraci[i]!);
                    if (i != length - 1) output += ",";
                    continue;
                }
#pragma warning restore IDE0150 // Prefer 'null' check over type check

                JsonNode node = koraci[i]!; // Korak kojeg cemo ispisati, bio on referenca ili ne

                // Ako je korak zapravo referenca, imamo posebnu proceduru:
                bool flag = true;
                while (flag && node["ref"] is JsonNode referenca)
                {
                    string path = referenca.ToString();
                    string fileName = path[..path.IndexOf('#')];

                    JsonNode root; // Dohvatit cemo korijen json dokumenta s imenom fileName
                    using (StreamReader r = new(fileName))
                    {
                        string jsonString = r.ReadToEnd();
                        root = JsonNode.Parse(jsonString)!.Root;
                    }

                    // Kroz putanju cemo prolaziti dio po dio, odnosno od slasha do slasha
                    // Varijabla indexLimit oznacavat ce desnu granicu dijela kojeg trenutno istrazujemo
                    int indexLimit;
                    int lastDetectedSlash = path.IndexOf('/');
                    if (lastDetectedSlash == -1) // Referenca ne sadrzi slash, dakle sastoji se samo od imena datoteke, simbola # i imena rjecnika
                    {
                        indexLimit = path.Length;
                    }
                    else { indexLimit = lastDetectedSlash; }
                    string dictionaryName = path[(path.IndexOf('#') + 1)..indexLimit];
                    // U json dokumentu s danim imenom fileName potrazit cemo odgovarajuci rjecnik s imenom dictionaryName
                    node = PronadjiIzKorijena(root, dictionaryName);

                    if (lastDetectedSlash != -1) // Referenca je sadrzavala slash, odnosno nije se sastojala samo od imena datoteke, simbola # i imena rjecnika
                    {
                        while (lastDetectedSlash != path.LastIndexOf('/')) // Sve dok nismo procitali zadnji slash u pathu...
                        {
                            int nextSlash = path.IndexOf('/', lastDetectedSlash + 1);
                            string dictionaryKey = path[(lastDetectedSlash + 1)..nextSlash]; // Citamo dio patha izmedju zadnjeg otkrivenog slasha i iduceg neotkrivenog slasha

                            JsonArray listElements = node[dictionaryKey]!.AsArray(); // Dohvatili smo listu koja odgovara kljucu dictionaryKey (npr "sastojci" u pathu "a.json#X/sastojci/1")

                            lastDetectedSlash = nextSlash;
                            nextSlash = path.IndexOf('/', lastDetectedSlash + 1);
                            if (nextSlash == -1) nextSlash = path.Length;
                            int index = Convert.ToInt32(path[(lastDetectedSlash + 1)..nextSlash]); // Indeks elementa liste

                            node = listElements[index - 1]!; // - 1 jer kuharovo indeksiranje krece jedinicom

                            string str = node.ToString();
                            if (!str.Contains('{'))
                            {
                                output += "\"";
                                output += str;
                                output += "\"";
                                if (i != length - 1) output += ",";
                                flag = false;
                                break;
                            }

#pragma warning disable IDE0150 // Prefer 'null' check over type check
                            if (node["ref"] is JsonNode) continue;
#pragma warning restore IDE0150 // Prefer 'null' check over type check
                            else break;
                        }
                    }
                }

                if (!flag) continue;

                output += IspisiKorak(node);
                if (i != length - 1) output += ",";
            }
            else
            {
                output += "\"";
                output += koraci[i];
                output += "\"";
                if (i != length - 1) output += ",";
            }

        }
        output += "]";

        return output;
    }

    static string IspisiUpute(JsonNode uputeNode)
    {
        string output = "";

        output += "\"posluzivanje\":[";
        JsonArray upute = uputeNode.AsArray();
        int length = upute.Count;
        for (int i = 0; i < length; i++)
        {
            output += "\"";
            output += upute[i]!;
            output += "\"";
            if (i != length - 1) output += ",";
        }
        output += "]";

        return output;
    }
};