
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Ekmansoft.FamilyTree.Tools.CompareResults
{
  public class NameEquivalences
  {
    [DataMember]
    public string baseName { get; set; }
    [DataMember]
    public IList<string> equivalentNames { get; set; }

    public NameEquivalences(string baseName)
    {
      equivalentNames = new List<string>();
      this.baseName = baseName.ToLower();
    }
    public void AddEquivalent(string variant)
    {
      equivalentNames.Add(variant.ToLower());
    }
    public bool IsEquivalent(string name)
    {
      if (name.ToLower() == baseName)
      {
        return true;
      }
      foreach (string eq in equivalentNames)
      {
        if (eq == name.ToLower())
        {
          return true;
        }
      }
      return false;
    }
  }

  public class NameEquivalenceDb
  {
    private static TraceSource trace = new TraceSource("NameEquivalents", SourceLevels.Warning);
    [DataMember]
    public IDictionary<string, NameEquivalences> equivalentNames { get; set; }

    private static readonly string NameDbDatabaseFilename = ".nameequivalencedb.json";

    public static string GetDefaultFilePath()
    {
      string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      if (basePath.Length == 0)
      {
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
      }
      if (basePath.Length == 0)
      {
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
      }
      if (basePath.Length == 0)
      {
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      }
      if (basePath.Length > 0)
      {
        basePath = basePath + "/";
      }
      return basePath + NameDbDatabaseFilename;
    }

    public NameEquivalenceDb()
    {
      equivalentNames = new Dictionary<string, NameEquivalences>();
    }
    public void AddEquivalent(string baseName, string equivalence)
    {
      if (!equivalentNames.ContainsKey(baseName))
      {
        equivalentNames.Add(baseName, new NameEquivalences(baseName));
      }
      NameEquivalences variant;
      if (equivalentNames.TryGetValue(baseName, out variant))
      {
        variant.AddEquivalent(equivalence);
        equivalentNames.Remove(variant.baseName);
        equivalentNames.Add(baseName, variant);
      }
    }
    static string NormalizeName(string name)
    {
      return name.ToLower().Replace("  ", " ").Replace("*", "").Replace("(", "").Replace(")", "");
    }

    public string SimplifyName(string fullName)
    {

      IList<string> subNames = NormalizeName(fullName).Split(" ");
      IList<string> resultNames = new List<string>();

      foreach (string name in subNames)
      {
        string lName = name.ToLower();
        bool found = false;
        foreach (string eq in equivalentNames.Keys)
        {
          if (lName == eq)
          {
            //return name.ToLower();
            resultNames.Add(lName);
            found = true;
            break;
          }
        }
        if (!found)
        {
          foreach (NameEquivalences eq in equivalentNames.Values)
          {
            if (eq.IsEquivalent(lName))
            {
              resultNames.Add(eq.baseName);
              found = true;
              break;
            }
          }
        }
        if (!found)
        {
          resultNames.Add(lName);
        }
      }
      if (string.Join(" ", resultNames) != fullName)
      {
        trace.TraceData(TraceEventType.Information, 0, "Name [" + fullName + "] simplifies to [" + string.Join(" ", resultNames) + "]");
      }
      return string.Join(" ", resultNames);
    }
    public static NameEquivalenceDb LoadFile(string filename)
    {
      try
      {
        using (StreamReader r = new StreamReader(filename))
        {
          string json = r.ReadToEnd();
          NameEquivalenceDb fileDb = FromJson(json);
          if (fileDb != null)
          {
            fileDb.PrintDb();
          } else
          {
            trace.TraceInformation("Name db read from " + filename + " failed");
          }
          return fileDb;
        }
      }
      catch (System.IO.FileNotFoundException e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "File read fnfe " + filename + " failed" + e.ToString());
      }
      catch (System.Exception e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "File read " + filename + " failed" + e.ToString());
      }
      finally 
      {
        trace.TraceData(TraceEventType.Warning, 0, "File read " + filename + " failed maybe");
      }
      return null;
    }
    public static bool SaveFile(string filename, NameEquivalenceDb db)
    {
      try
      {
        using (StreamWriter wr = new StreamWriter(filename))
        {
          string dbJson = NameEquivalenceDb.ToJson(db);
          wr.Write(dbJson);
          wr.Close();
          trace.TraceData(TraceEventType.Information, 0, "File write " + filename + " done");
        }
        return true;
      }
      catch (DirectoryNotFoundException e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "File write dnfe " + filename + " failed" + e.ToString());
      }
      catch (System.Exception e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "File write " + filename + " failed" + e.ToString());
      }
      return false;
    }
    public void LoadDefault()
    {
      AddEquivalent("adolf", "adolph");

      AddEquivalent("gustaf", "gustav");

      AddEquivalent("mikael", "michael");

      AddEquivalent("kristina", "christina");
      AddEquivalent("kristina", "stina");
      AddEquivalent("kristina", "cristina");

      AddEquivalent("katarina", "catarina");
      AddEquivalent("katarina", "catharina");

      AddEquivalent("karl", "carl");
      AddEquivalent("karl", "kalle");

      AddEquivalent("klara", "clara");

      AddEquivalent("kristian", "christian");
      AddEquivalent("kristian", "cristian");

      AddEquivalent("oskar", "oscar");

      AddEquivalent("valdemar", "waldemar");

      AddEquivalent("manfrid", "manfred");

      AddEquivalent("eriksson", "ericsson");
      AddEquivalent("eriksson", "eriksdotter");

      AddEquivalent("nilsson", "nilsdotter");

      AddEquivalent("per", "pehr");
      AddEquivalent("per", "p�hr");

      AddEquivalent("persson", "pehrsson");
      AddEquivalent("persson", "p�hrsson");
      AddEquivalent("persson", "p�rsson");
      AddEquivalent("persson", "pehrson");
      AddEquivalent("persson", "persdotter");
      AddEquivalent("persson", "pehrsdotter");
      PrintDb();
    }

    void PrintDb()
    {
      trace.TraceData(TraceEventType.Information, 0, "eq-names:" + equivalentNames.Count);

      foreach (NameEquivalences eq in equivalentNames.Values)
      {
        trace.TraceData(TraceEventType.Information, 0, "base:" + eq.baseName);
        trace.TraceData(TraceEventType.Information, 0, "eq  :" + string.Join(",", eq.equivalentNames));
      }
    }
    public static NameEquivalenceDb FromJson(string json)
    {
      return JsonSerializer.Deserialize<NameEquivalenceDb>(json);
    }

    public static string ToJson(NameEquivalenceDb o)
    {
      return JsonSerializer.Serialize(o);
    }

  }

  /*
     * Artur:Arthur
     * Karl:Carl,Calle,Kalle
     * Klara:Clara
     * Kristina:Stina,Cristina,Christina
     * Kristian:Christian,Cristian
     * Manfred:Manfrid
     * Per:Pehr
     * Oskar:Oscar
     * Valdemar:Waldemar
     * Eriksson:Ericsson
     * Noren:Nor�n
     * Persson:Pehrsson,Pehrson
     * 
     */

  public class DefaultNameEquivalenceDb : NameEquivalenceDb
  {
    public DefaultNameEquivalenceDb()
    {
      //NameEquivalences stina = new NameEquivalences();

      //stina.baseName = "kristina";
      //stina.equivalentNames.Add("stina");
      //stina.equivalentNames.Add("christina");
      //stina.equivalentNames.Add("cristina");
      //equivalentNames.Add(stina);

      //NameEquivalences karl = new NameEquivalences();
      //karl.baseName = "karl";
      //karl.equivalentNames.Add("carl");
      //karl.equivalentNames.Add("kalle");
      //equivalentNames.Add(karl);

      //NameEquivalences kristian = new NameEquivalences();
      //kristian.baseName = "kristian";
      //kristian.equivalentNames.Add("christian");
      //kristian.equivalentNames.Add("cristian");
      //equivalentNames.Add(kristian);

      //NameEquivalences eriksson = new NameEquivalences();
      //eriksson.baseName = "eriksson";
      //eriksson.equivalentNames.Add("ericsson");
      //equivalentNames.Add(eriksson);

      //NameEquivalences per = new NameEquivalences();
      //per.baseName = "per";
      //per.equivalentNames.Add("pehr");
      //equivalentNames.Add(per);

      //NameEquivalences persson = new NameEquivalences();
      //persson.baseName = "persson";
      //persson.equivalentNames.Add("pehrsson");
      //equivalentNames.Add(persson);
      //equivalentNames.AddEquivalent("kristina", "christina");
    }


  }
}