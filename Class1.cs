using System;
using System.Linq;
using System.Text;
using RegExp = System.Text.RegularExpressions;
using Xml = System.Xml;

//!Todo?: Legge inn kontrollsjekk på at innputverdiene er på riktig format. Dette vil gjøre programmet mer brukervennlig, men samtidig tregere. Noen funksjoner har kontrollsjekk, men endel har ikke det.
namespace DiverseFunksjoner
{
    /// <summary>
    /// Diverse hjelpefiler.
    /// </summary>
    public class Class1
    {
        //private Li[] array;

        /// <summary>
        /// Leser beskrivelser av funksjonen og variablene gitt i funkVarNavnLi, fra en C# xml fil.
        /// Merk at bare variablene gitt i funkVarNavnLi får beskrivelser.
        /// Første element er funksjonsbeskrivelsen. Resten er variabelbeskrivelsene, som kommer i samme rekkefølge som variablene i funkVarNavnLi.
        /// </summary>
        /// <param name="funkVarNavnLi">Array med navnet på funksjonen og hvilke av variablene dens som man vil ha beskrivelse for.</param>
        /// <param name="fil">Full path med filnavn til xml filen.</param>
        /// <returns>Første element er funksjonsbeskrivelsen. Resten er variabelbeskrivelsene, som kommer i samme rekkefølge som variablene i funkVarNavnLi.</returns>
        public static string[] XmlLesFunkVarBesk(string[] funkVarNavnLi, string fil)
        {
            string[,] funkVarNavnBeskLi = XmlLesVarBeskFraFunk(funkVarNavnLi[0], fil); //Henter full variabelliste for funksjonen.
            if (funkVarNavnBeskLi == null) return null;
            string[] funkVarBeskLi = new string[funkVarNavnLi.Length];
            funkVarBeskLi[0] = funkVarNavnBeskLi[0, 1];
            for (int i = 1; i < funkVarNavnLi.Length; i++)
            {
                int i2;
                for (i2 = 1; i2 < funkVarNavnBeskLi.GetLength(0); i2++) if (funkVarNavnLi[i] == funkVarNavnBeskLi[i2, 0]) break;
                if (i2 == funkVarNavnBeskLi.GetLength(0)) Console.WriteLine("Fant ikke funksjonen " + funkVarNavnLi[0] + " sin variabel " + funkVarNavnLi[i] + " i xmlfilen.");
                else funkVarBeskLi[i] = funkVarNavnBeskLi[i2, 1];
            }
            return funkVarBeskLi;
        }

        /// <summary>
        /// Leser navn og beskrivelser av funksjonen og alle dens variable, fra en C# xml fil.
        /// Dobbelarrayen har to kolonner. Første kolonne er navn, mens andre kolonne er beskrivelser. Rad en inneholder info om funksjonen, mens resten av radene er info om variablene.
        /// </summary>
        /// <param name="funkNavn">Navnet på funksjonen man vil ha info om.</param>
        /// <param name="fil">Full path med filnavn til xml filen.</param>
        /// <returns>Dobbelarrayen har to kolonner. Første kolonne er navn, mens andre kolonne er beskrivelser. Rad en inneholder info om funksjonen, mens resten av radene er info om variablene.</returns>
        public static string[,] XmlLesVarBeskFraFunk(string funkNavn, string fil)
        {
            Xml.XmlTextReader rd = new Xml.XmlTextReader(fil);
            System.Collections.Generic.List<string> funkVarBeskLi = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<string> funkVarNavnLi = new System.Collections.Generic.List<string>();
            bool iFunk = false;
            string reg = "([.]" + funkNavn + "+([(]))"; //matcher punktum, etterfulgt av funksjonsnavnet, etterfulgt av parentesstart.
            while (rd.Read())
            {
                if (rd.Name == "member")
                { //Funksjonsnavn
                    if (iFunk) break; //Ferdig med funksjonen.
                    string attr = "";
                    for (int j = 0; j < rd.AttributeCount; j++) attr = rd.GetAttribute(0); //Hvis jeg ikke har med for-løkken, så får jeg en feilmelding.
                    if (RegExp.Regex.Match(attr, reg).Success)
                    {
                        iFunk = true;
                        funkVarNavnLi.Add(funkNavn);
                    }
                }
                else if (iFunk && rd.Name == "param")
                { //Variabelnavn og beskrivelse
                    funkVarNavnLi.Add(rd.GetAttribute(0).Trim());
                    funkVarBeskLi.Add(rd.ReadString().Trim());
                }
                else if (iFunk && rd.Name == "summary")
                { //Funksjonsbeskrivelse
                    string attr = rd.ReadString();
                    Parser.Parser p = new Parser.Parser(attr);
                    string[] funkBeskLi = p.ReadStrings('\n'); //Kvitter meg med whitespace etter linjeskift.
                    attr = "";
                    foreach (string s in funkBeskLi) attr += s.Trim() + " "; //Erstatter linjeskift+whitespace med et mellomrom. Kan alternativt erstatte det med et linjeskift uten whitespace.
                    funkVarBeskLi.Add(attr.Trim());
                }
            }
            rd.Close();
            if (!iFunk)
            {
                Console.WriteLine("Fant ikke funksjonen " + funkVarNavnLi[0] + " i xmlfilen.");
                return null;
            }
            string[,] funkVarNavnBeskLi = new string[funkVarBeskLi.Count, 2];
            for (int i = 0; i < funkVarNavnBeskLi.GetLength(0); i++)
            {
                funkVarNavnBeskLi[i, 0] = funkVarNavnLi[i];
                funkVarNavnBeskLi[i, 1] = funkVarBeskLi[i];
            }
            return funkVarNavnBeskLi;
        }

        /// <summary>
        /// Finner alle funksjoner som en bestemt variabel inngår i, og variabelens tilhørende beskrivelse. Hentes fra en C# xml fil.
        /// Dobbelarrayen har to kolonner. Første kolonne er funksjonsnavn, mens andre kolonne er variabelbeskrivelsen for hver funksjon.
        /// </summary>
        /// <param name="varNavn">Navnet på variabelen man vil ha info om.</param>
        /// <param name="fil">Full path med filnavn til xml filen.</param>
        /// <returns>Dobbelarrayen har to kolonner. Første kolonne er funksjonsnavn, mens andre kolonne er variabelbeskrivelsen for hver funksjon.</returns>
        public static string[,] XmlLesFunkNavnVarBeskFraVar(string varNavn, string fil)
        {
            Xml.XmlTextReader rd = new Xml.XmlTextReader(fil);
            System.Collections.Generic.List<string> varBeskLi = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<string> funkNavnLi = new System.Collections.Generic.List<string>();
            string reg = "([.]\\w+([(]))"; //matcher punktum, etterfulgt av et ord, etterfulgt av parentesstart.
            while (rd.Read())
            {
                if (rd.Name == "member")
                { //Funksjonsnavn
                    string attr = "";
                    for (int j = 0; j < rd.AttributeCount; j++) attr = rd.GetAttribute(0); //Hvis jeg ikke har med for-løkken, så får jeg en feilmelding.
                    RegExp.Match m = RegExp.Regex.Match(attr, reg);
                    if (m.Success)
                    {
                        attr = m.ToString();
                        funkNavnLi.Add(attr.Remove(attr.Length - 1).Remove(0, 1)); //Fjerner punktum og parentes
                    }
                }
                else if (rd.Name == "param")
                { //Variabelbeskrivelse
                    string attr = "";
                    if (rd.AttributeCount > 0) attr = rd.GetAttribute(0).Trim(); //Det hender at rd ikke har noen attributes.
                    if (attr == varNavn) varBeskLi.Add(rd.ReadString().Trim());
                }
            }
            rd.Close();
            if (string.IsNullOrEmpty(funkNavnLi[0]))
            {
                Console.WriteLine("Fant ikke variabelen " + varNavn + " i xmlfilen.");
                return null;
            }
            string[,] funkNavnVarBeskLi = new string[funkNavnLi.Count, 2];
            for (int i = 0; i < funkNavnVarBeskLi.GetLength(0); i++)
            {
                funkNavnVarBeskLi[i, 0] = funkNavnLi[i];
                funkNavnVarBeskLi[i, 1] = varBeskLi[i];
            }
            return funkNavnVarBeskLi;
        }

        /// <summary>
        /// Finner antall ganger en frase dukker opp i en tekst.
        /// </summary>
        /// <param name="tekst">Teksten som skal søkes gjennom.</param>
        /// <param name="frase">Frasen som skal finnes.</param>
        /// <returns>Antall fraser i teksten.</returns>
        public static int FraseAntall(string tekst, string frase)
        {
            if (string.IsNullOrEmpty(tekst)) return 0;
            int antall = 0;
            int fraseLengde = frase.Length;
            if (tekst.Contains(frase))
            {
                int tegn = 0;
                while (tegn <= tekst.Length - fraseLengde)
                {
                    if (string.Equals(tekst.Substring(tegn, fraseLengde), frase, StringComparison.Ordinal))
                    {
                        tegn += fraseLengde;
                        antall++;
                    }
                    else tegn++;
                }
            }
            return antall;
        }

        /// <summary>
        /// Finner antall forekomster av et tegn i en streng.
        /// </summary>
        /// <param name="streng">Teksten som skal gjennomsøkes.</param>
        /// <param name="verdi">Tegnet som skal telles.</param>
        /// <returns>Antall tegn.</returns>
        public static int TegnAntall(string streng, char verdi)
        {
            int antall = 0;
            foreach (char tegn in streng) if (tegn == verdi) antall++;
            return antall;
        }

        /// <summary>
        /// Legger til et anførselstegn for hvert anførselstegn i en tekst. (" -> "")
        /// Brukes når en skal skrive en streng til en annen programmeringsfil.
        /// </summary>
        /// <param name="tekst">Teksten som skal gjennomsøkes.</param>
        /// <returns>Den redigerte teksten.</returns>
        public static string AnførselKorrigering(string tekst)
        {
            int lengde = tekst.Length;
            for (int i = 0; i < lengde; i++)
            {
                if (tekst[i] == '"')
                {
                    tekst = tekst.Insert(i++, "\"");
                    lengde++;
                    //tekst = tekst.Insert(i, "\\\"\\"); //!Jeg har ikke opplevd noe forskjell på å ha denne linjen med eller ikke. \\ på slutten av linjen er der for å escape det andre anførselstegnet.
                    //i += 3;
                    //lengde += 3;
                }
            }
            return tekst;
        }

        //Trenger å ha doble skråstreker ettersom strengene skrives ut til fil, hvor de så blir kjørt.
        /// <summary>
        /// Legger til en backslash for hver backslash i en tekst. (\ -> \\)
        /// Brukes når en skal skrive en streng til en annen programmeringsfil.
        /// </summary>
        /// <param name="streng">Teksten som skal gjennomsøkes.</param>
        /// <returns>Den redigerte teksten.</returns>
        public static string SkråstrekKorrigering(string streng)
        {
            int lengde = streng.Length;
            for (int i = 0; i < lengde; i++)
            {
                if (streng[i] == '\\')
                {
                    streng = streng.Insert(i, "\\");
                    i++;
                    lengde++;
                }
                else if (streng[i] == '\n')
                {
                    streng = streng.Remove(i, 1);
                    streng = streng.Insert(i, "\\n");
                    i += 1;
                    lengde += 1;
                }
            }
            return streng;
        }

        //!Sjekk denne.
        //public static int mod(int a, int b) { int c = a % b; return c < 0 ? c + b : c; }

        /// <summary>
        /// Tar modolus av negative tall. (Mod(-6,8) returnerer 2)
        /// </summary>
        /// <param name="verdi">Tallet man tar modulus på.</param>
        /// <param name="div">Dividenden.</param>
        /// <returns>Resultatet av modulus operasjonen.</returns>
        public static int Mod(int verdi, int div)
        {
            //For å ta modulus av negative tall.
            //Mod(-0.6,0.8) returnerer 0.2
            if (verdi == 0) return 0;
            if (div == 0)
            {
                Console.WriteLine("Prøver å ta modulus med 0 som divisor");
                return 0;
            }
            if (verdi > 0)
            {
                if (div > 0) while (verdi > div) verdi -= div;
                else while (verdi > 0) verdi += div;
            }
            else
            {
                if (div > 0) while (verdi < 0) verdi += div;
                else while (verdi < div) verdi -= div;
            }
            return verdi;
        }

        /// <summary>
        /// Tar modus av negative tall. (Mod(-0.6,0.8) returnerer 0.2)
        /// </summary>
        /// <param name="verdi">Tallet man tar modulus på.</param>
        /// <param name="div">Dividenden.</param>
        /// <returns>Resultatet av modulus operasjonen.</returns>
        public static double Mod(double verdi, double div)
        {
            //For å ta modulus av negative tall.
            //Mod(-0.6,0.8) returnerer 0.2
            if (verdi == 0) return 0;
            if (div == 0) return double.NaN;
            if (verdi > 0)
            {
                if (div > 0) while (verdi > div) verdi -= div;
                else while (verdi > 0) verdi += div;
            }
            else
            {
                if (div > 0) while (verdi < 0) verdi += div;
                else while (verdi < div) verdi -= div;
            }
            return verdi;
        }

        /// <summary>
        /// Skriver elementvis hvert element i li + ' ' til en streng.
        /// </summary>
        /// <param name="li">Arrayen som skal gjøres til en streng.</param>
        /// <returns>Den splittede strengen, hvor hvert element er separert med et mellomrom.</returns>
        public static string ArrayTilStreng(byte[] li)
        {
            string s = "";
            foreach (byte var in li) s += var + ' ';
            return s.Remove(s.Length - 1);
        }

        /// <summary>
        /// Fjerner elementer som er lik en gitt verdi fra en liste.
        /// </summary>
        /// <param name="li">Listen som skal redigeres.</param>
        /// <param name="verdi">Verdien som skal fjernes.</param>
        /// <param name="alle">false: Førse element = verdi fjernes. true: Alle elementer = verdi fjernes.</param>
        public static void FjernElementFraLi(ref int[] li, int verdi, bool alle)
        {
            int n = 0;
            int[] liTmp = new int[li.Length];
            bool fortsett = true;
            for (int i = 0; i < li.Length; i++)
            {
                if (fortsett && li[i] == verdi)
                {
                    n++;
                    if (alle) fortsett = false;
                }
                liTmp[i - n] = li[i];
            }
            li = new int[liTmp.Length - n];
            for (int i = 0; i < li.Length; i++) li[i] = liTmp[i];
        }

        /// <summary>
        /// Fjerner alle forekomster av en tekst fra en annen tekst.
        /// Virker på Null strenger.
        /// </summary>
        /// <param name="tekst">Teksten som skal redigeres.</param>
        /// <param name="slett">Det man skal slette.</param>
        public static void FjernStrengDel(ref string tekst, string slett)
        {
            if (string.IsNullOrEmpty(tekst) || string.IsNullOrEmpty(slett)) return;
            tekst = tekst.Replace(slett, "");
        }

        /// <summary>
        /// Fjerner alle forekomster av en tekst fra alle elementer i en array.
        /// Virker på Null strenger.
        /// </summary>
        /// <param name="tekst">Arrayen som skal redigeres.</param>
        /// <param name="slett">Det man skal slette.</param>
        public static void FjernStrengDel(ref string[] tekst, string slett)
        {
            for (int i = 0; i < tekst.Length; i++)
            {
                FjernStrengDel(ref tekst[i], slett);
            }
        }

        /// <summary>
        /// Fjerner alle forekomster av en tekst fra alle elementer i en dobbelarrary.
        /// Virker på Null strenger.
        /// </summary>
        /// <param name="tekst">Dobbelarraryen som skal redigeres.</param>
        /// <param name="slett">Det man skal slette.</param>
        public static void FjernStrengDel(ref string[,] tekst, string slett)
        {
            for (int i = 0; i < tekst.GetLength(0); i++) for (int j = 0; j < tekst.GetLength(1); j++) FjernStrengDel(ref tekst[i, j], slett);
        }

        //public static bool SammenlignArrays(Li[] array1, Li[] array2){
        //    if(array1.Length != array2.Length) return false;
        //    for(int i = 0; i < array1.Length; i++) if(array1[i] != array2[i]) return false;
        //    return true;
        //}

        /// <summary>
        /// Finner antall desimaler i et tall.
        /// Virker ikke med eksponenttall (3E-12).
        /// </summary>
        /// <param name="tall">Tallet som skal gjennomsøkes.</param>
        /// <returns>Antall desimaler.</returns>
        public static int DesimalAntall(string tall)
        {
            int desimalPlass = tall.IndexOf(',') + 1;
            if (desimalPlass <= 0) desimalPlass = tall.IndexOf(',') + 1;
            if (desimalPlass <= 0) desimalPlass = tall.Length;
            return tall.Length - desimalPlass;
        }
        /// <summary>
        /// Finner antall desimaler i et tall.
        /// Virker ikke med eksponenttall (3E-12).
        /// </summary>
        /// <param name="tall">Tallet som skal gjennomsøkes.</param>
        /// <returns>Antall desimaler.</returns>
        public static int DesimalAntall(double tall)
        {
            return DesimalAntall(tall.ToString());
        }

        /// <summary>
        /// Sammenligner to flyttall på følgende måte:
        /// |(a - b)/a| mindre enn 10^-14 (Forskjell på 0,000000000001%) returnerer true.
        /// a = 0 -> |b| mindre enn 10^-14 returnerer true.
        /// </summary>
        /// <param name="a">Tall 1.</param>
        /// <param name="b">Tall 2.</param>
        /// <returns>true hvis forskjellen er mindre enn 10^-12%.</returns>
        public static bool SammenlignFlyttallTilnærmet(double a, double b)
        {
            return SammenlignFlyttallTilnærmet(a, b, 0.00000000000001);
        }
        /// <summary>
        /// Sammenligner to flyttall på følgende måte:
        /// |(a - b)/a| mindre enn tillattForskjell returnerer true.
        /// a = 0 -> |b| mindre enn tillattForskjell returnerer true.
        /// </summary>
        /// <param name="a">Tall 1.</param>
        /// <param name="b">Tall 2.</param>
        /// <param name="tillattForskjell">Valgfri. 0 betyr helt likt, 0.1 tillater en forskjell på 10%. Default er 0.00000000000001.</param>
        /// <returns>true hvis forskjellen er mindre enn tillattForskjell.</returns>
        public static bool SammenlignFlyttallTilnærmet(double a, double b, double tillattForskjell)
        {
            if (a == 0)
            {
                if (Math.Abs(b) < tillattForskjell) return true;
            }
            else if (Math.Abs((a - b) / a) < tillattForskjell) return true;
            return false;
        }
        /// <summary>
        /// Sammenligner to flyttall på følgende måte:
        /// |(a - b)/a| mindre enn 10^(-tillattForskjellEksponent) returnerer true.
        /// a = 0 -> |b| mindre enn 10^(-tillattForskjellEksponent) returnerer true.
        /// </summary>
        /// <param name="a">Tall 1.</param>
        /// <param name="b">Tall 2.</param>
        /// <param name="tillattForskjellEksponent">Valgfri. Tillatt forskjell: 0 -> 100% tillatt forskjell, 4 -> 0.01% tillatt forskjell. Default er 14.</param>
        /// <returns>true hvis forskjellen er mindre enn 10^(-tillattForskjellEksponent).</returns>
        public static bool SammenlignFlyttallTilnærmet(double a, double b, int tillattForskjellEksponent)
        {
            double e = 10 ^ -tillattForskjellEksponent;
            if ((a - b) / a < e) return true;
            else return false;
        }

        /// <summary>
        /// Fjerner VB kommentarer fra en streng. (Søker etter ' som ikke er i en streng eller escape'et.)
        /// </summary>
        /// <param name="streng">Teksten som skal redigeres.</param>
        public static void FjernVBKommentar(ref string streng)
        {
            char t = '\'';
            bool tekst = false;
            int i;
            for (i = 0; i < streng.Length; i++)
            {
                if (streng[i] == '"')
                { //Er ikke sikker på om dette og muligens \ er de eneste stedene hvor '-kommentar kan forekomme.
                    if (!tekst) tekst = true;
                    else tekst = false;
                }
                else if (!tekst)
                {
                    if (streng[i] == '\\') i++; //Hopper over tegnet etter escape tegnet \. Jeg er ikke sikker på om \ kan forekomme utenfor en streng-tekst.
                    else if (streng[i] == t)
                    {
                        streng = streng.Remove(i);
                        break;
                    }
                }
            }
        }

        //Sjekker om postene i excelfilen stemmer stemmer med forventede poster.
        /// <summary>
        /// Sjekker om elementene i første raden i en dobbelarray er lik de respektive elementene i en array.
        /// </summary>
        /// <param name="data">Matrise som skal kontrolleres.</param>
        /// <param name="tekstTittel">Array som skal være lik første raden i data.</param>
        /// <returns>true hvis første raden i data er lik tekstTittel.</returns>
        public static bool TittelSjekk(object[,] data, string[] tekstTittel)
        {
            if (data.GetLength(1) != tekstTittel.Length)
            {
                Console.WriteLine("Antall titler i excelfilen og programmet stemmer ikke overens. Leste " + data.GetLength(1) + " titler, forventet " + tekstTittel.Length + " titler.");
                return false;
            }
            for (int i = 0; i < tekstTittel.Length; i++)
            {
                if (!string.Equals(data[i, 0].ToString(), tekstTittel[i], StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Titlene i excelfilen stemmer ikke med forventede titler. Tittel i excelfil: " + data[i, 1] + ", forventet tittel: " + tekstTittel[i - 2]);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Leter gjennom den første raden i en dobbelarray etter en streng, og returnerer kolonnenummeret (0-basis) til strengen.
        /// </summary>
        /// <param name="data">Dobbelarrayen som skal gjennomsøkes.</param>
        /// <param name="post">Strengen som skal finnes.</param>
        /// <param name="inneholder">true hvis feltene i data bare trenger å inneholde post. false hvis feltet må være eksakt lik post.</param>
        /// <returns>Kolonnenummeret til post (0-basis).</returns>
        public static int FinnPostKol(object[,] data, string post, bool inneholder)
        {
            for (int i = 0; i < data.GetLength(1); i++)
            {
                if (!inneholder)
                {
                    if (string.Equals(data[0, i].ToString(), post, StringComparison.Ordinal)) return i;
                }
                else if (data[0, i].ToString().Contains(post)) return i;
            }
            Console.WriteLine("Kunne ikke finne posten " + post + ".");
            return -1;
        }

        /// <summary>
        /// Gjør om alle tall i en dobbelarray til integere.
        /// </summary>
        /// <param name="data">Dobbelarrayen som skal redigeres.</param>
        public static void GjørTilInt(ref object[,] data)
        {
            double tmp;
            for (int i = 1; i <= data.GetLength(0); i++) for (int j = 1; j <= data.GetLength(1); j++) if (data[i, j] != null) if (double.TryParse(data[i, j].ToString(), out tmp)) data[i, j] = (int)Math.Floor(tmp);
        }

        /// <summary>
        /// Returnerer navnet på en måned.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <returns>Månedsnavn.</returns>
        public static string MånedNavn(string dato)
        {
            int måned = int.Parse(dato.Remove(5, 5).Remove(0, 3));
            return MånedNavn(måned);
        }

        /// <summary>
        /// Returnerer navnet på en måned.
        /// </summary>
        /// <param name="måned">Månedsnummeret (1-12).</param>
        /// <returns>Månedsnavn.</returns>
        public static string MånedNavn(int måned)
        {
            string[] månedLi = new string[] { "januar", "februar", "mars", "april", "Mai", "juni", "juli", "august", "september", "oktober", "november", "desember" };
            return månedLi[måned - 1];
        }

        /// <summary>
        /// Bytter om rekkefølgen på dato fra dd.mm.åååå til åååå.mm.dd
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <returns>Dato snudd til åååå.mm.dd</returns>
        public static string DatoSnu(string dato)
        {
            string[] datoSplittet = DatoSplittDmå(dato);
            return datoSplittet[2] + "." + datoSplittet[1] + "." + datoSplittet[0];
        }

        /// <summary>
        /// Bytter om rekkefølgen på dato fra dmå til gitt format.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <param name="RekkefølgeDmå">Format: en av følgende: åmd, ådm, måd, mdå, dmå, dåm, åm, åd, må, md, dm, då, d, m, å (å kan erstattes med y).</param>
        /// <returns>Snudd dato.</returns>
        public static string DatoSnu(string dato, string RekkefølgeDmå)
        {
            //Format på dato er: dd.mm.åååå . Har ikke lagt inn noen kontrollsjekk på om den er på riktig format.
            RekkefølgeDmå.Replace('y', 'å');
            string[] datoSplittet = DatoSplittDmå(dato);
            string[] rekkefølgeTillatt = new string[] { "åmd", "ådm", "måd", "mdå", "dmå", "dåm", "åm", "åd", "må", "md", "dm", "då", "d", "m", "å" };
            if (!rekkefølgeTillatt.Contains(RekkefølgeDmå.ToLower()))
            {
                char c = RekkefølgeDmå[0];
                int lengde = RekkefølgeDmå.Length;
                for (int i = 1; i < lengde; i++)
                {
                    if (RekkefølgeDmå[i] == c)
                    {
                        RekkefølgeDmå = RekkefølgeDmå.Remove(i, 1);
                        lengde--;
                    }
                    else c = RekkefølgeDmå[i];
                }
                if (!rekkefølgeTillatt.Contains(RekkefølgeDmå.ToLower()))
                {
                    Console.Write("åmdRekkefølge har feil format. Forventet format er en av følgende: " + RekkefølgeDmå[0]);
                    for (int i = 1; i < RekkefølgeDmå.Length; i++) Console.Write(" - " + RekkefølgeDmå[i]);
                    Console.WriteLine();
                    return "";
                }
            }
            string datoSnudd = "";
            foreach (char c in RekkefølgeDmå.ToLower())
            {
                if (c == 'å') datoSnudd += "." + datoSplittet[2];
                else if (c == 'm') datoSnudd += "." + datoSplittet[1];
                else if (c == 'd') datoSnudd += "." + datoSplittet[0];
            }
            return datoSnudd.Remove(0, 1);
        }

        /// <summary>
        /// Splitter en datostreng i tre: dag, måned, år.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <returns>Array med lengde tre: dag, måned, år.</returns>
        public static string[] DatoSplittDmå(string dato)
        {
            //Format på dato er: dd.mm.åååå . Har ikke lagt inn noen kontrollsjekk på om den er på riktig format.
            string[] datoSplittet = new string[3];
            datoSplittet[0] = dato.Remove(2); //Dag
            datoSplittet[1] = dato.Remove(5, 5).Remove(0, 3); //Måned
            datoSplittet[2] = dato.Remove(0, dato.Length - 4); //År
            return datoSplittet;
        }

        /// <summary>
        /// Endrer dato med antall dager, måneder og år. Bruker C# sin DateTime funksjon for å avgjøre hvor mange dager det er i februar.
        /// Ved måneds/års endring: hvis dag er siste dag i forige måned, så blir ny dag siste dag i ny måned (31.01.2009 -> 28.02.2009).
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <param name="diffDag">Antall dager som skal adderes/subtraheres.</param>
        /// <param name="diffMåned">Antall måneder som skal adderes/subtraheres.</param>
        /// <param name="diffÅr">Antall år som skal adderes/subtraheres.</param>
        /// <returns>Den redigerte datoen.</returns>
        public static string DatoEndring(string dato, int diffDag, int diffMåned, int diffÅr)
        {
            StringBuilder tmp = new StringBuilder();
            return DatoEndring(dato, diffDag, diffMåned, diffÅr, ref tmp);
        }

        /// <summary>
        /// Endrer dato med antall dager, måneder og år. Bruker C# sin DateTime funksjon for å avgjøre hvor mange dager det er i februar.
        /// Ved måneds/års endring: hvis dag er siste dag i forige måned, så blir ny dag siste dag i ny måned (31.01.2009 -> 28.02.2009).
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <param name="diffDag">Antall dager som skal adderes/subtraheres.</param>
        /// <param name="diffMåned">Antall måneder som skal adderes/subtraheres.</param>
        /// <param name="diffÅr">Antall år som skal adderes/subtraheres.</param>
        /// <param name="feilMelding">Valgfri. Tar vare på feilmeldingene.</param>
        /// <returns>Den redigerte datoen.</returns>
        public static string DatoEndring(string dato, int diffDag, int diffMåned, int diffÅr, ref StringBuilder feilMelding)
        {
            //Format på dato er: dd.mm.åååå . Har ikke lagt inn noen kontrollsjekk på om den er på riktig format.
            //Koden er ikke særlig pen, og bør kunne skrives penere, men man risikerer da å miste noe fleksibilitet.
            //Det er også diskutabelt hvordan man skal la dagen være når man endrer på måneden og året. (Hvis dagen er 31 og måneden blir skiftet til februar f.eks.)
            int dag = int.Parse(dato.Remove(2));
            int måned = int.Parse(dato.Remove(5, 5).Remove(0, 3));
            int år = int.Parse(dato.Remove(0, dato.Length - 4));
            bool sisteDag = false;
            if (dag == DateTime.DaysInMonth(år, måned)) sisteDag = true; //Skal sørge for at den tilblivende datoen også er siste dag i måneden.
            dag += diffDag;
            måned += diffMåned;
            år += diffÅr;
            if (måned <= 0 || måned > 12)
            {
                år += (int)Math.Ceiling(måned / 12.0) - 1;
                måned = (int)Mod(måned, 12);
                if (måned == 0) måned = 12;
            }
            if (sisteDag) dag = DateTime.DaysInMonth(år, måned);
            while (dag <= 0 || dag > DateTime.DaysInMonth(år, måned))
            {
                if (diffDag == 0 && dag > 0) dag = DateTime.DaysInMonth(år, måned); //Betyr at måneden har skiftet til en månede med færre dager enn den opprinnelige måneden, slik at dag er høyere enn maks antall dager i den nåværende måneden. Et alternativ her er å la måneden skifte til neste månede, men jeg føler at det er mer naturlig å la dag skifte til siste dagen i måneden.
                if (dag <= 0)
                {
                    if (--måned == 0) dag += DateTime.DaysInMonth(år - 1, 12); //Måned vil nå bli null, og DaysInMonth forventer en måned mellom 1 og 12.
                    else dag += DateTime.DaysInMonth(år, måned);
                }
                else dag -= DateTime.DaysInMonth(år, måned++);
                if (måned <= 0 || måned > 12) år += (int)Math.Ceiling(måned / 12.0) - 1;
                måned = (int)Mod(måned, 12);
                if (måned == 0) måned = 12;
            }
            if (dag == 0) dag = DateTime.DaysInMonth(år, måned);
            //Trenger linjene nedenfor siden 0 i begynelsen av dag, måned år fjernes ved overføring til/fra int.
            string dagStr = dag.ToString();
            string månedStr = måned.ToString();
            string årStr = år.ToString();
            if (dagStr.Length == 1) dagStr = "0" + dagStr;
            if (månedStr.Length == 1) månedStr = "0" + månedStr;
            if (årStr.Length < 4) for (int i = årStr.Length; i < 4; i++) årStr = "0" + årStr;
            else if (årStr.Length > 4)
            {
                Console.WriteLine("Årstallet (" + årStr + ") etter datoendring er for stort. Tillatt lengde på 4 tegn (format dd.mm.åååå), lengde på " + årStr.Length + " tegn funnet.");
                feilMelding.AppendLine("Årstallet (" + årStr + ") etter datoendring er for stort. Tillatt lengde på 4 tegn (format dd.mm.åååå), lengde på " + årStr.Length + " tegn funnet.");
            }
            return dagStr + "." + månedStr + "." + årStr;
        }

        /// <summary>
        /// Sjekker at dato er på format dd.mm.ååå, med riktige måneds- og dags- verdier.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <returns>true hvis dato er på riktig format.</returns>
        public static bool DatoSjekkFormat(string dato)
        {
            StringBuilder tmp = new StringBuilder();
            return DatoSjekkFormat(dato, ref tmp);
        }
        /// <summary>
        /// Sjekker at dato er på format dd.mm.ååå, med riktige måneds- og dags- verdier.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <param name="feilMelding">Valgfri. Tar vare på feilmeldingene.</param>
        /// <returns>true hvis dato er på riktig format.</returns>
        public static bool DatoSjekkFormat(string dato, ref StringBuilder feilMelding)
        {
            if (dato.Length != 10)
            {
                Console.WriteLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                feilMelding.AppendLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                return false;
            }
            int år; if (!int.TryParse(dato.Remove(0, dato.Length - 4), out år))
            {
                Console.WriteLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                feilMelding.AppendLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                return false;
            }
            int måned; if (!int.TryParse(dato.Remove(5, 5).Remove(0, 3), out måned))
            {
                Console.WriteLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                feilMelding.AppendLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                return false;
            }
            int dag; if (!int.TryParse(dato.Remove(2), out dag))
            {
                Console.WriteLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                feilMelding.AppendLine("Dato stemmer ikke. Gitt dato: " + dato + ", forventet format: dd.mm.åååå");
                return false;
            }
            if (måned > 12 || måned < 1)
            {
                Console.WriteLine("Dato stemmer ikke. Måned må være mellom 1 og 12 (" + måned + " gitt). Gitt dato: " + dato);
                feilMelding.AppendLine("Dato stemmer ikke. Måned må være mellom 1 og 12 (" + måned + " gitt). Gitt dato: " + dato);
                return false;
            }
            if (dag < 1 || dag > DateTime.DaysInMonth(år, måned))
            {
                Console.WriteLine("Dato stemmer ikke. Dag må være mellom 1 og " + DateTime.DaysInMonth(år, måned) + " (" + dag + " gitt). Gitt dato: " + dato);
                feilMelding.AppendLine("Dato stemmer ikke. Dag må være mellom 1 og " + DateTime.DaysInMonth(år, måned) + " (" + dag + " gitt). Gitt dato: " + dato);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sjekker at dato er på format dd.mm.ååå, med riktige måneds- og dags- verdier, og at dag er siste dagen i måneden.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <returns>true hvis dato er på riktig format.</returns>
        public static bool DatoSjekkSisteDag(string dato)
        {
            StringBuilder tmp = new StringBuilder();
            return DatoSjekkSisteDag(dato, ref tmp);
        }

        /// <summary>
        /// Sjekker at dato er på format dd.mm.ååå, med riktige måneds- og dags- verdier, og at dag er siste dagen i måneden.
        /// </summary>
        /// <param name="dato">Format: dd.mm.åååå</param>
        /// <param name="feilMelding">Valgfri. Tar vare på feilmeldingene.</param>
        /// <returns>true hvis dato er på riktig format.</returns>
        public static bool DatoSjekkSisteDag(string dato, ref StringBuilder feilMelding)
        {
            if (!DatoSjekkFormat(dato, ref feilMelding)) return false;
            int måned; int.TryParse(dato.Remove(5, 5).Remove(0, 3), out måned);
            int dag; int.TryParse(dato.Remove(2), out dag);
            int år; int.TryParse(dato.Remove(0, dato.Length - 4), out år);
            if (dag != DateTime.DaysInMonth(år, måned))
            {
                Console.WriteLine("Dato stemmer ikke. Dag må være siste dagen i måneden (" + DateTime.DaysInMonth(år, måned) + ", " + dag + " gitt). Gitt dato: " + dato);
                feilMelding.AppendLine("Dato stemmer ikke. Dag må være siste dagen i måneden (" + DateTime.DaysInMonth(år, måned) + ", " + dag + " gitt). Gitt dato: " + dato);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Regner ut n!.
        /// </summary>
        /// <param name="n">Verdien som det skal tas fakultet av.</param>
        /// <returns>Verdien til n!.</returns>
        public static double Matte_Fakultet(int n)
        {
            bool pos = true;
            if (n < 0)
            {
                pos = true;
                n *= -1;
            }
            double verdi = 1;
            for (int i = 2; i <= n; i++) verdi *= i;
            if (!pos) verdi *= -1;
            return verdi;
        }

        /// <summary>
        /// Regner ut nCr (kombinasjoner av uordnet utvalg uten tilbakelegging.
        /// </summary>
        /// <param name="n">Antall elementer totalt.</param>
        /// <param name="r">Antall elementer som skal trekkes ut fra n.</param>
        /// <returns>Antall uordnede utvalg uten tilbakelegging.</returns>
        public static double Matte_nCr(int n, int r)
        {
            if (r > n) throw new Exception("r cannot be bigger than n.");
            return Matte_Fakultet(n) / (Matte_Fakultet(r) * Matte_Fakultet(n - r));
            //double teller;
            //double nevner;
            //if(r == n) return 1;
            //else if(2*r > n){
            //    teller = r+1;
            //    r = n-r;
            //}else{
            //    teller = n-r+1;
            //}
            //for(int i = (int)teller + 1; i <= n; i++) teller *= i;
            //nevner = Matte_Fakultet(r);
            //return teller / nevner;
        }
    }
}

