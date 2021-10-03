using FamilyTreeLibrary.FamilyData;
using FamilyTreeLibrary.FamilyTreeStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace FamilyTreeTools.CompareResults
{
  class FamilyTreeCompareResults
  {
  }


  [DataContract]
  public class DuplicateTreeItems
  {
    [DataMember]
    public string item1;
    [DataMember]
    public string item2;

    public DuplicateTreeItems(string i1, string i2)
    {
      item1 = i1;
      item2 = i2;
    }
  }

  [DataContract]
  public class SavedMatches
  {
    [DataMember]
    public string database1, database2;
    [DataMember]
    public IList<DuplicateTreeItems> itemList;

    public SavedMatches()
    {
      itemList = new List<DuplicateTreeItems>();
    }
  }

  public delegate void ReportCompareResult(IFamilyTreeStoreBaseClass familyTree1, string person1, IFamilyTreeStoreBaseClass familyTree2, string person2);

  public class CompareTreeClass
  {
    private static TraceSource trace = new TraceSource("CompareTrees", SourceLevels.Warning);

    public class NameEquivalences
    {
      public string baseName;
      public IList<string> equivalentNames;

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
      public IDictionary<string,NameEquivalences> equivalentNames;

      public NameEquivalenceDb()
      {
        equivalentNames = new Dictionary<string,NameEquivalences>();
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
          equivalentNames.Add(baseName, variant);
        }
      }

      public string SimplifyName(string fullName)
      {
        IList<string> subNames = fullName.Split(" ");
        IList<string> resultNames = new List<string>();

        foreach (string name in subNames)
        {
          string lName = name.ToLower();
          foreach (string eq in equivalentNames.Keys)
          {
            if (lName == eq)
            {
              //return name.ToLower();
              resultNames.Add(lName);
            }
          }
          foreach (NameEquivalences eq in equivalentNames.Values)
          {
            if (eq.IsEquivalent(name))
            {
              resultNames.Add(eq.baseName);
            }
          }
        }
        trace.TraceInformation("Name [" + fullName + "] simplifies to [" + string.Join(" ", resultNames) + "]");
        return string.Join(" ", resultNames);
      }
      public void LoadDefault()
      {
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
        AddEquivalent("per", "pähr");

        AddEquivalent("persson", "pehrsson");
        AddEquivalent("persson", "pährsson");
        AddEquivalent("persson", "pärsson");
        AddEquivalent("persson", "pehrson");
        AddEquivalent("persson", "persdotter");
        AddEquivalent("persson", "pehrsdotter");

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
     * Noren:Norén
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

    static string NormalizeName(string name)
    {
      return name.ToLower().Replace("w", "v").Replace("  ", " ").Replace("*", "").Replace("å", "a").Replace("ä", "a").Replace("ö", "o").Replace("é", "e");
    }

    static bool IsNamesEqual(string name1, string name2, NameEquivalenceDb nameEqDb)
    {
      name1 = NormalizeName(name1);
      name1 = nameEqDb.SimplifyName(name1);

      name2 = NormalizeName(name2);
      name2 = nameEqDb.SimplifyName(name2);

      return name1 == name2;
    }

    enum DateMatch
    {
      Unknown,
      Bad,
      Ok,
      Good
    }
    static DateMatch MatchDates(FamilyDateTimeClass date1, FamilyDateTimeClass date2)
    {
      if ((date1 != null) && (date2 != null))
      {
        if (date1.ValidDate() && date2.ValidDate())
        {
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonthDay) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonthDay))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            TimeSpan diff = dt1 - dt2;

            if (Math.Abs(diff.Days) <= 10)
            {
              return DateMatch.Good;
            }
            if (Math.Abs(diff.Days) <= 300)
            {
              return DateMatch.Ok;
            }
            return DateMatch.Bad;
          }
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonth) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.YearMonth))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            if ((dt1.Year == dt2.Year) && (dt1.Month == dt2.Month))
            {
              return DateMatch.Good;
            }
            if (Math.Abs(dt1.Year - dt2.Year) <= 1)
            {
              return DateMatch.Ok;
            }
            return DateMatch.Bad;
          }
          if ((date1.GetDateType() >= FamilyDateTimeClass.FamilyDateType.Year) &&
              (date2.GetDateType() >= FamilyDateTimeClass.FamilyDateType.Year))
          {
            DateTime dt1 = date1.ToDateTime();
            DateTime dt2 = date2.ToDateTime();

            if (dt1.Year == dt2.Year)
            {
              return DateMatch.Good;
            }
            if (Math.Abs(dt1.Year - dt2.Year) <= 2)
            {
              return DateMatch.Ok;
            }
          }
          return DateMatch.Bad;
        }
      }
      return DateMatch.Unknown;
    }

    public static bool ComparePerson(IndividualClass person1, IndividualClass person2, NameEquivalenceDb nameEqDb)
    {
      if (IsNamesEqual(person1.GetName(), person2.GetName(), nameEqDb))
      {
        IndividualEventClass birth1 = person1.GetEvent(IndividualEventClass.EventType.Birth);
        IndividualEventClass birth2 = person2.GetEvent(IndividualEventClass.EventType.Birth);
        IndividualEventClass death1 = person1.GetEvent(IndividualEventClass.EventType.Death);
        IndividualEventClass death2 = person2.GetEvent(IndividualEventClass.EventType.Death);

        DateMatch birthMatch = DateMatch.Unknown, deathMatch = DateMatch.Unknown;

        if ((birth1 != null) && (birth2 != null))
        {
          birthMatch = MatchDates(birth1.GetDate(), birth2.GetDate());
        }
        if ((death1 != null) && (death2 != null))
        {
          deathMatch = MatchDates(death1.GetDate(), death2.GetDate());
        }
        if ((birthMatch == DateMatch.Unknown) && (deathMatch == DateMatch.Unknown))
        {
          return false;
        }
        if ((birthMatch == DateMatch.Bad) || (deathMatch == DateMatch.Bad))
        {
          return false;
        }
        return (birthMatch == DateMatch.Good) || (deathMatch == DateMatch.Good);
      }
      return false;
    }


    public static void SearchDuplicates(IndividualClass person1, IFamilyTreeStoreBaseClass familyTree1, IFamilyTreeStoreBaseClass familyTree2, ReportCompareResult reportDuplicate, IProgressReporterInterface reporter = null, NameEquivalenceDb nameEqDb = null)
    {
      IndividualEventClass birth = person1.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass death = person1.GetEvent(IndividualEventClass.EventType.Death);

      if (reporter != null)
      {
        trace.TraceInformation(reporter.ToString());
      }
      if (((birth != null) && (birth.GetDate() != null) && (birth.GetDate().ValidDate())) ||
          ((death != null) && (death.GetDate() != null) && (death.GetDate().ValidDate())))
      {
        string searchString;

        if (familyTree2.GetCapabilities().jsonSearch)
        {
          searchString = SearchDescriptor.ToJson(SearchDescriptor.GetSearchDescriptor(person1));
        }
        else
        {
          searchString = person1.GetName().Replace("*", "");
        }

        IEnumerator<IndividualClass> iterator2 = familyTree2.SearchPerson(searchString);
        int cnt2 = 0;

        if (iterator2 != null)
        {
          int cnt3 = 0;
          do
          {
            IndividualClass person2 = iterator2.Current;

            if (person2 != null)
            {
              cnt3++;
              //trace.TraceInformation(reporter.ToString() + "   2:" + person2.GetName());
              if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
              {
                if (ComparePerson(person1, person2, nameEqDb))
                {
                  trace.TraceData(TraceEventType.Information, 0, "   2:" + person2.GetName() + " " + person1.GetXrefName() + " " + person2.GetXrefName());
                  reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                }
                cnt2++;
              }
            }
          } while (iterator2.MoveNext());

          iterator2.Dispose();
          trace.TraceInformation(" " + searchString + " matched with " + cnt2 + "," + cnt3);
        }

        if (cnt2 == 0) // No matches found for full name
        {
          if ((person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname).Length > 0) &&
              (person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname).Length > 0) &&
              !person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname).Equals(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname)))
          {
            String strippedName = person1.GetName().Replace("*", "");

            if (strippedName.Contains(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname)))
            {
              String maidenName = strippedName.Replace(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.Surname), "").Replace("  ", " ");
              IEnumerator<IndividualClass> iterator3 = familyTree2.SearchPerson(maidenName);
              //trace.TraceInformation(" Searching Maiden name " + maidenName);

              if (iterator3 != null)
              {
                int cnt3 = 0;
                do
                {
                  IndividualClass person2 = iterator3.Current;

                  if (person2 != null)
                  {
                    if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
                    {
                      cnt3++;
                      if (ComparePerson(person1, person2, nameEqDb))
                      {
                        trace.TraceData(TraceEventType.Information, 0, "   2b:" + person2.GetName());
                        reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                      }
                    }
                  }
                } while (iterator3.MoveNext());
                iterator3.Dispose();
                trace.TraceInformation(" Maiden name " + maidenName + " mathched with " + cnt3);
              }
            }
            if (strippedName.Contains(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname)))
            {
              String marriedName = strippedName.Replace(person1.GetPersonalName().GetName(PersonalNameClass.PartialNameType.BirthSurname), "").Replace("  ", " ");
              IEnumerator<IndividualClass> iterator3 = familyTree2.SearchPerson(marriedName);

              //trace.TraceInformation(" Searching Married name " + marriedName);
              if (iterator3 != null)
              {
                int cnt3 = 0;
                do
                {
                  //IndividualClass person1 = iterator1.Current;
                  IndividualClass person2 = iterator3.Current;

                  if (person2 != null)
                  {
                    //trace.TraceInformation(reporter.ToString() + "   2:" + person2.GetName());
                    if ((familyTree1 != familyTree2) || (person1.GetXrefName() != person2.GetXrefName()))
                    {
                      cnt3++;
                      if (ComparePerson(person1, person2, nameEqDb))
                      {
                        trace.TraceData(TraceEventType.Information, 0, "   2c:" + person2.GetName());
                        reportDuplicate(familyTree1, person1.GetXrefName(), familyTree2, person2.GetXrefName());
                      }
                    }
                  }
                } while (iterator3.MoveNext());
                iterator3.Dispose();
                trace.TraceInformation(" Married name " + marriedName + " matched to " + cnt3);
              }
            }
          }
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Information, 0, "No valid birth or death date for " + person1.GetName().ToString() + " skip duplicate search");
      }
    }

    public static void CompareTrees(IFamilyTreeStoreBaseClass familyTree1, IFamilyTreeStoreBaseClass familyTree2, ReportCompareResult reportDuplicate, IProgressReporterInterface reporter = null)
    {
      IEnumerator<IndividualClass> iterator1;
      int cnt1 = 0;

      NameEquivalenceDb equivDb = new DefaultNameEquivalenceDb();
      equivDb.LoadDefault();

      iterator1 = familyTree1.SearchPerson(null, reporter);

      trace.TraceData(TraceEventType.Information, 0, "CompareTrees() started");

      if (iterator1 != null)
      {
        do
        {
          IndividualClass person1 = iterator1.Current;

          cnt1++;
          if (person1 != null)
          {
            trace.TraceData(TraceEventType.Information, 0, " 1:" + cnt1 + " " + person1.GetName());
            SearchDuplicates(person1, familyTree1, familyTree2, reportDuplicate, reporter, equivDb);
          }
          else
          {
            trace.TraceData(TraceEventType.Warning, 0, " 1: person is null" + cnt1);
          }
        } while (iterator1.MoveNext());
        iterator1.Dispose();
      }
      else
      {
        trace.TraceInformation("iter=null");
      }
      trace.TraceInformation("CompareTrees() done");
    }
  }
}
