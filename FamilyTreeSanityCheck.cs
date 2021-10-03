using FamilyTreeLibrary.FamilyData;
using FamilyTreeLibrary.FamilyTreeStore;
//using FamilyTreeLibrary.FamilyTreeStore;
//using System.Threading.Tasks;
using FamilyTreeTools.CompareResults;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FamilyTreeTools.FamilyTreeSanityCheck
{

  public class FamilyStatusClass
  {
    public enum EventCorrectness
    {
      None,
      Semi,
      Perfect,
      BadFormat,
      Unknown
    };

    static public EventCorrectness CheckEvent(IndividualClass person, IndividualEventClass.EventType evType)
    {
      IndividualEventClass ev = person.GetEvent(evType);
      if (ev != null)
      {
        FamilyDateTimeClass date = ev.GetDate();
        if (date != null)
        {
          if (date.ValidDate())
          {
            if (!date.GetApproximate())
            {
              switch (date.GetDateType())
              {
                case FamilyDateTimeClass.FamilyDateType.YearMonthDayHourMinute:
                case FamilyDateTimeClass.FamilyDateType.YearMonthDayHourMinuteSecond:
                case FamilyDateTimeClass.FamilyDateType.YearMonthDayHour:
                case FamilyDateTimeClass.FamilyDateType.YearMonthDay:
                  return EventCorrectness.Perfect;

                default:
                  return EventCorrectness.Semi;
              }
            }
          }
          else if (date.GetDateType() == FamilyDateTimeClass.FamilyDateType.DateString)
          {
            return EventCorrectness.BadFormat;
          }
        }
      }

      return EventCorrectness.None;
    }

    public class IndividualStatus
    {
      public int noOfParents;
      public int noOfChildren;

      public EventCorrectness birthCorrectness;
      public EventCorrectness deathCorrectness;

      public IndividualStatus()
      {
        noOfParents = 0;
        noOfChildren = 0;
        birthCorrectness = EventCorrectness.Unknown;
        deathCorrectness = EventCorrectness.Unknown;
      }
    }

    static public IndividualStatus CheckCorrectness(IFamilyTreeStoreBaseClass familyTree, IndividualClass person)
    {
      IndividualStatus status = new();

      if (person != null)
      {
        status.birthCorrectness = CheckEvent(person, IndividualEventClass.EventType.Birth);
        status.deathCorrectness = CheckEvent(person, IndividualEventClass.EventType.Death);
        {
          IList<FamilyXrefClass> childFams = person.GetFamilyChildList();

          if (childFams != null)
          {
            foreach (FamilyXrefClass famXref in childFams)
            {
              FamilyClass family = familyTree.GetFamily(famXref.GetXrefName());

              if (family != null)
              {
                IList<IndividualXrefClass> parentList = family.GetParentList();
                if (parentList != null)
                {
                  status.noOfParents += parentList.Count;
                }
              }
            }
          }
        }
        {
          IList<FamilyXrefClass> spouseFams = person.GetFamilySpouseList();

          if (spouseFams != null)
          {
            foreach (FamilyXrefClass famXref in spouseFams)
            {
              FamilyClass family = familyTree.GetFamily(famXref.GetXrefName());

              if (family != null)
              {
                IList<IndividualXrefClass> childList = family.GetChildList();
                if (childList != null)
                {
                  status.noOfChildren += childList.Count;
                }
              }
            }
          }
        }
      }
      return status;
    }

  }

  [DataContract]
  public class SanityProperty
  {
    [DataMember]
    public bool active;
    [DataMember]
    public int value;
  }

  [DataContract]
  public class SanityCheckLimits
  {
    [DataMember]
    public int generationsBack;
    [DataMember]
    public int generationsForward;

    [DataMember]
    public SanityProperty parentLimitMin;
    [DataMember]
    public SanityProperty motherLimitMax;
    [DataMember]
    public SanityProperty fatherLimitMax;
    [DataMember]
    public SanityProperty eventLimitMin;
    [DataMember]
    public SanityProperty eventLimitMax;
    [DataMember]
    public SanityProperty noOfChildrenMin;
    [DataMember]
    public SanityProperty noOfChildrenMax;
    [DataMember]
    public SanityProperty daysBetweenChildren;
    [DataMember]
    public SanityProperty twins;
    [DataMember]
    public SanityProperty inexactBirthDeath;
    [DataMember]
    public SanityProperty unknownBirth;
    [DataMember]
    public SanityProperty unknownDeath;
    [DataMember]
    public SanityProperty unknownDeathEmigrated;
    [DataMember]
    public SanityProperty parentsMissing;
    [DataMember]
    public SanityProperty parentsProblem;
    [DataMember]
    public SanityProperty missingWeddingDate;
    [DataMember]
    public SanityProperty marriageProblem;
    [DataMember]
    public SanityProperty missingPartner;
    [DataMember]
    public SanityProperty missingPartnerMitigated;
    [DataMember]
    public SanityProperty generationlimited;
    [DataMember]
    public SanityProperty duplicateCheck;
    [DataMember]
    public SanityProperty unknownSex;
    [DataMember]
    public SanityProperty endYear;
    [DataMember]
    public SanityProperty oldPrivateProfile;
    [DataMember]
    public SanityProperty shortAddress;
    [DataMember]
    public SanityProperty unknownGpsPosition;

    public IDictionary<SanityProblemId, SanityProperty> sanityArray;

    public enum SanityProblemId
    {
      parentLimitMin_e,
      motherLimitMax_e,
      fatherLimitMax_e,
      eventLimitMin_e,
      eventLimitMax_e,
      noOfChildrenMin_e,
      noOfChildrenMax_e,
      daysBetweenChildren_e,
      twins_e,
      inexactBirthDeath_e,
      //unknownBirthDeath_e,
      unknownBirth_e,
      unknownDeath_e,
      unknownDeathEmigrated_e,
      parentsMissing_e,
      parentsProblem_e,
      marriageProblem_e,
      missingWeddingDate_e,
      missingPartner_e,
      generationlimited_e,
      duplicateCheck_e,
      unknownSex_e,
      endYear_e,
      oldPrivateProfile_e,
      shortAddress_e,
      unknownGpsPosition_e,
      missingPartnerMitigated_e,
    }

    public SanityCheckLimits()
    {
      generationsBack = 5;
      generationsForward = 2;

      parentLimitMin = new SanityProperty();
      parentLimitMin.value = 15;
      parentLimitMin.active = true;

      motherLimitMax = new SanityProperty();
      motherLimitMax.value = 48;
      motherLimitMax.active = true;

      fatherLimitMax = new SanityProperty();
      fatherLimitMax.active = true;
      fatherLimitMax.value = 65;

      eventLimitMin = new SanityProperty();
      eventLimitMin.active = true;
      eventLimitMin.value = 0;

      eventLimitMax = new SanityProperty();
      eventLimitMax.active = true;
      eventLimitMax.value = 105;

      noOfChildrenMin = new SanityProperty();
      noOfChildrenMin.active = true;
      noOfChildrenMin.value = 1;

      noOfChildrenMax = new SanityProperty();
      noOfChildrenMax.value = 15;
      noOfChildrenMax.active = true;

      daysBetweenChildren = new SanityProperty();
      daysBetweenChildren.value = 250;
      daysBetweenChildren.active = true;

      twins = new SanityProperty();
      twins.active = true;

      inexactBirthDeath = new SanityProperty();
      inexactBirthDeath.active = true;

      unknownBirth = new SanityProperty();
      unknownBirth.active = true;

      unknownDeath = new SanityProperty();
      unknownDeath.active = true;

      unknownDeathEmigrated = new SanityProperty();
      unknownDeathEmigrated.active = true;

      parentsMissing = new SanityProperty();
      parentsMissing.active = true;

      parentsProblem = new SanityProperty();
      parentsProblem.active = true;

      missingWeddingDate = new SanityProperty();
      missingWeddingDate.active = true;

      marriageProblem = new SanityProperty();
      marriageProblem.active = true;

      missingPartner = new SanityProperty();
      missingPartner.value = 115;
      missingPartner.active = true;

      missingPartnerMitigated = new SanityProperty();
      missingPartnerMitigated.value = 115;
      missingPartnerMitigated.active = true;

      generationlimited = new SanityProperty();
      generationlimited.active = false;

      duplicateCheck = new SanityProperty();
      duplicateCheck.active = true;

      unknownSex = new SanityProperty();
      unknownSex.active = true;

      oldPrivateProfile = new SanityProperty();
      oldPrivateProfile.active = true;

      shortAddress = new SanityProperty();
      shortAddress.active = true;

      unknownGpsPosition = new SanityProperty();
      unknownGpsPosition.active = true;

      endYear = new SanityProperty();
      endYear.value = 1900;
      endYear.active = true;

      CreateArray();
    }

    public void CreateArray()
    {
      sanityArray = new Dictionary<SanityProblemId, SanityProperty>();
      sanityArray.Add(SanityProblemId.parentLimitMin_e, parentLimitMin);
      sanityArray.Add(SanityProblemId.motherLimitMax_e, motherLimitMax);
      sanityArray.Add(SanityProblemId.fatherLimitMax_e, fatherLimitMax);
      sanityArray.Add(SanityProblemId.eventLimitMin_e, eventLimitMin);
      sanityArray.Add(SanityProblemId.eventLimitMax_e, eventLimitMax);
      sanityArray.Add(SanityProblemId.noOfChildrenMin_e, noOfChildrenMin);
      sanityArray.Add(SanityProblemId.noOfChildrenMax_e, noOfChildrenMax);
      sanityArray.Add(SanityProblemId.daysBetweenChildren_e, daysBetweenChildren);
      sanityArray.Add(SanityProblemId.twins_e, twins);
      sanityArray.Add(SanityProblemId.inexactBirthDeath_e, inexactBirthDeath);
      sanityArray.Add(SanityProblemId.unknownBirth_e, unknownBirth);
      sanityArray.Add(SanityProblemId.unknownDeath_e, unknownDeath);
      sanityArray.Add(SanityProblemId.unknownDeathEmigrated_e, unknownDeathEmigrated);
      sanityArray.Add(SanityProblemId.parentsMissing_e, parentsMissing);
      sanityArray.Add(SanityProblemId.parentsProblem_e, parentsProblem);
      sanityArray.Add(SanityProblemId.marriageProblem_e, marriageProblem);
      sanityArray.Add(SanityProblemId.missingWeddingDate_e, missingWeddingDate);
      sanityArray.Add(SanityProblemId.missingPartner_e, missingPartner);
      sanityArray.Add(SanityProblemId.missingPartnerMitigated_e, missingPartnerMitigated);
      sanityArray.Add(SanityProblemId.generationlimited_e, generationlimited);
      sanityArray.Add(SanityProblemId.duplicateCheck_e, duplicateCheck);
      sanityArray.Add(SanityProblemId.unknownSex_e, unknownSex);
      sanityArray.Add(SanityProblemId.endYear_e, endYear);
      sanityArray.Add(SanityProblemId.oldPrivateProfile_e, oldPrivateProfile);
      sanityArray.Add(SanityProblemId.shortAddress_e, shortAddress);
      sanityArray.Add(SanityProblemId.unknownGpsPosition_e, unknownGpsPosition);
    }
  }

  [DataContract]
  public class Relation
  {
    public enum Type
    {
      Person,
      Woman,
      Man,
      Mother,
      Father,
      Parent,
      Daughter,
      Son,
      Child,
      Spouse,
      Sibling,
      Same,
      Unknown
    }

    [DataMember]
    public Type type;
    [DataMember]
    public string personXref;
    [DataMember]
    public string name;
    [DataMember]
    public string url;
    [DataMember]
    public string birth;
    [DataMember]
    public string death;

    public Relation(Type type, string personXref, string name, string url, string birth, string death)
    {
      this.type = type;
      this.personXref = personXref;
      this.name = name;
      this.url = url;
      this.birth = birth;
      this.death = death;
    }

    public static Relation MakeRelation(IndividualClass person, Relation.Type type)
    {
      string url = "";
      IList<string> urls = person.GetUrlList();
      if (urls.Count > 0)
      {
        url = urls[0];
      }
      return new Relation(type, person.GetXrefName(), person.GetName(), url,
          AncestorStatistics.GetEventDateString(person, IndividualEventClass.EventType.Birth),
          AncestorStatistics.GetEventDateString(person, IndividualEventClass.EventType.Death));
    }

    public override string ToString()
    {
      return this.type + ":" + this.personXref + ":" + this.name + " (" + this.birth + " - " + this.death + ")";
    }
    public string ToString(bool showRelation, bool html)
    {
      StringBuilder builder = new();

      if (showRelation)
      {
        builder.Append(this.type + ":");
      }
      if (html && url.Length > 0)
      {
        builder.Append("<a href=\"" + url + "\">");
      }
      builder.Append(name);
      if (html && url.Length > 0)
      {
        builder.Append("</a>");
      }

      builder.Append(" (" + birth + " - " + death + ")");

      return builder.ToString();
    }
    static public Relation.Type GetChildRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Daughter;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Son;
        default:
          return Relation.Type.Child;

      }
    }
    static public Relation.Type GetParentRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Mother;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Father;
        default:
          return Relation.Type.Parent;

      }
    }
    static public Relation.Type GetSex(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Woman;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Man;
        default:
          return Relation.Type.Person;
      }
    }

  }

  [CollectionDataContract]
  public class RelationStack : List<Relation>
  {
    public class RelDistance
    {
      public int ancestorGen = 0;
      public int descendantGen = 0;
      public int marriageNo = 0;

      public RelDistance(int ancestors, int descendants, int marriage)
      {
        this.ancestorGen = ancestors;
        this.descendantGen = descendants;
        this.marriageNo = marriage;
      }
    }
    public string ToString(bool html)
    {
      StringBuilder strBuilder = new();
      strBuilder.Append(CalculateRelation(html) + Linefeed(html));


      foreach (Relation relation in this)
      {
        strBuilder.Append("  " + relation.ToString(true, html) + Linefeed(html));
      }
      return strBuilder.ToString();
    }
    private static string Linefeed(bool html = false)
    {
      if (html)
      {
        return "<br/>" + FamilyUtility.GetLinefeed();
      }
      return FamilyUtility.GetLinefeed();
    }

    public RelationStack Duplicate()
    {
      RelationStack stack = new();

      foreach (Relation rel in this)
      {
        stack.Add(rel);
      }
      return stack;
    }
    public RelDistance Distance()
    {
      int ancestorGen = 0;
      int descendantGen = 0;
      int marriageNo = 0;
      foreach (Relation relation in this)
      {
        switch (relation.type)
        {
          case Relation.Type.Father:
          case Relation.Type.Mother:
          case Relation.Type.Parent:
            ancestorGen++;
            break;

          case Relation.Type.Son:
          case Relation.Type.Daughter:
          case Relation.Type.Child:
            descendantGen++;
            break;
          case Relation.Type.Spouse:
            marriageNo++;
            break;
        }
      }
      return new RelDistance(ancestorGen, descendantGen, marriageNo);
    }

    public string GetDistance()
    {
      StringBuilder resultStr = new();

      RelDistance dist = Distance();

      if (dist.ancestorGen > 0)
      {
        resultStr.Append("a:" + dist.ancestorGen);
      }
      if (dist.descendantGen > 0)
      {
        resultStr.Append(" d:" + dist.descendantGen);
      }
      if (dist.marriageNo > 0)
      {
        resultStr.Append(" m:" + dist.marriageNo);
      }
      return resultStr.ToString();
    }
    public string GetLast()
    {
      if (this.Count > 0)
      {
        return this[this.Count - 1].personXref;
      }
      return "";
    }
    public string GetFirst()
    {
      if (this.Count > 0)
      {
        return this[0].personXref;
      }
      return "";
    }
    public void RemoveLast()
    {
      if (this.Count > 0)
      {
        this.RemoveAt(this.Count - 1);
      }
    }
    public string CalculateRelation(bool html)
    {
      int rootIndex = 0;

      for (int i = 0; i < this.Count; i++)
      {
        switch (this[i].type)
        {
          case Relation.Type.Father:
          case Relation.Type.Mother:
          case Relation.Type.Parent:
            {
              rootIndex = i;
            }
            break;
        }
      }

      int minGenerations = Math.Min(rootIndex, this.Count - rootIndex - 1);
      int maxGenerations = Math.Max(rootIndex, this.Count - rootIndex - 1);
      int diffGenerations = maxGenerations - minGenerations;
      bool directAncestor = false;
      string str = "";

      switch (minGenerations)
      {
        case 0:
          if (minGenerations == maxGenerations)
          {
            str = "the same person";
          }
          else
          {
            //str = "Direct ancestor";
            directAncestor = true;
          }
          break;
        case 1:
          if (diffGenerations == 0)
          {
            str = "sibling";
          }
          else
          {
            if (rootIndex == 1)
            {
              switch (this[Count - 1].type)
              {
                case Relation.Type.Woman:
                case Relation.Type.Mother:
                case Relation.Type.Daughter:
                  str = "niece";
                  break;
                case Relation.Type.Man:
                case Relation.Type.Father:
                case Relation.Type.Son:
                case Relation.Type.Parent:
                  str = "nephew";
                  break;
                default:
                  str = "error:";
                  break;
              }

            }
            else if (rootIndex == (Count - 2))
            {
              switch (this[0].type)
              {
                case Relation.Type.Mother:
                case Relation.Type.Woman:
                case Relation.Type.Daughter:
                  str = "aunt";
                  break;
                case Relation.Type.Man:
                case Relation.Type.Father:
                case Relation.Type.Son:
                case Relation.Type.Parent:
                  str = "uncle";
                  break;
                default:
                  str = "error:";
                  break;
              }

            }
            else
            {
              str = "error:";
            }
            diffGenerations--;

          }
          break;
        case 2:
          str = "cousin";
          break;
        case 3:
          str = "second cousin";
          break;
        case 4:
          str = "third cousin";
          break;
        case 5:
          str = "fourth cousin";
          break;
        default:
          str = (minGenerations - 1) + "-th cousin";
          break;
      }
      if (diffGenerations != 0)
      {
        switch (diffGenerations)
        {
          case 1:
            str += " once removed";
            break;
          case 2:
            str += " twice removed";
            break;
          default:
            str += " " + diffGenerations + " generations removed";
            break;
        }
      }
      if (!directAncestor)
      {
        str += " with common ancestor: " + this[rootIndex].name + "(" + this[rootIndex].birth + " - " + this[rootIndex].birth + ")";
      }
      else
      {
        str += " direct descendant from " + this[rootIndex].name + "(" + this[rootIndex].birth + " - " + this[rootIndex].birth + ")";
      }
      return str;
    }

  }

  [DataContract]
  public class RelationStackList
  {
    [DataMember]
    public string sourceTree;
    [DataMember]
    public List<RelationStack> relations;
    [DataMember]
    public DateTime time;

    public RelationStackList()
    {
      relations = new List<RelationStack>();
    }

    private static string Linefeed(bool html = false)
    {
      if (html)
      {
        return "<br/>" + FamilyUtility.GetLinefeed();
      }
      return FamilyUtility.GetLinefeed();
    }

    public string ToString(IFamilyTreeStoreBaseClass familyTree, bool html)
    {
      StringBuilder strBuilder = new();

      if (html)
      {
        strBuilder.Append("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"/><title> Relations </title ></head ><body>" + Linefeed());
      }

      if (sourceTree != null)
      {
        strBuilder.Append(sourceTree);
      }
      strBuilder.Append(Linefeed(html));
      strBuilder.Append(time.ToString());
      strBuilder.Append(Linefeed(html));

      foreach (RelationStack rel in relations)
      {
        strBuilder.Append(rel.ToString(html));
      }
      if (html)
      {
        strBuilder.Append("</body></html>" + Linefeed());
      }

      return strBuilder.ToString();
    }
  }

  [DataContract]
  public class SanityProblem
  {
    [DataMember]
    public SanityCheckLimits.SanityProblemId id;
    [DataMember]
    public string details;
    [DataMember]
    public string url;

    public SanityProblem(SanityCheckLimits.SanityProblemId id, string details, string url = null)
    {
      this.id = id;
      this.details = details;
      this.url = url;
    }
  }

  [DataContract]
  public class JobInfo
  {
    [DataMember]
    public int JobId;
    [DataMember]
    public int Profiles;
    [DataMember]
    public int Families;
    [DataMember]
    public DateTime StartTime;
    [DataMember]
    public DateTime EndTime;

    [DataMember]
    public ICollection<AncestorLineInfo> IssueList;

    public JobInfo()
    {
      IssueList = new List<AncestorLineInfo>();
    }
  }


  [DataContract]
  public class AncestorLineInfo
  {
    private static readonly TraceSource trace = new("AncestorLineInfo", SourceLevels.Warning);

    [DataMember]
    public int depth;
    [DataMember]
    public string rootAncestor;
    [DataMember]
    public string name;
    [DataMember]
    public string url;
    [DataMember]
    public string sex;
    [DataMember]
    public string birth;
    [DataMember]
    public string death;
    //[DataMember]
    //public string details;
    [DataMember]
    public RelationStack relationPath;
    [DataMember]
    public IList<string> duplicate;
    [DataMember]
    public IList<SanityProblem> problemList;

    public AncestorLineInfo(string xref, string name, string url, string sex, string birth, string death, RelationStack relationStack, int depth, SanityCheckLimits.SanityProblemId id, string detailString, string duplicateUrl)
    {
      this.depth = depth;
      this.rootAncestor = xref;
      this.name = name;
      this.sex = sex;
      this.url = url;
      this.birth = birth;
      this.death = death;
      //this.details = detailString;
      if (problemList == null)
      {
        problemList = new List<SanityProblem>();
      }
      if (this.duplicate == null)
      {
        this.duplicate = new List<string>();
      }
      if (duplicateUrl != null)
      {
        this.duplicate.Add(duplicateUrl);
      }
      problemList.Add(new SanityProblem(id, detailString, duplicateUrl));

      // Depth no longer the same as number of generations
      /*if (depth != relationStack.Generations())
      {
        trace.TraceEvent(TraceEventType.Error, 0, "Error: Generation depth mismatch: " + depth + " " + relationStack.Count + " = " + relationStack.Generations());
      }*/
      if (relationStack != null)
      {
        this.relationPath = relationStack.Duplicate();
        if (relationStack.GetLast() != xref)
        {
          trace.TraceEvent(TraceEventType.Error, 0, "Error: Last person mismatch: " + relationStack.GetLast() + "!=" + xref + "; " + relationStack.GetDistance());
          trace.TraceEvent(TraceEventType.Error, 0, relationStack.ToString());
        }
      }
    }
    public string GetDetailString(SanityCheckLimits limits = null)
    {
      StringBuilder result = new();

      foreach (SanityProblem problem in problemList)
      {
        if ((limits == null) || limits.sanityArray[problem.id].active)
        {
          if (result.Length > 0)
          {
            result.Append("; ");
          }
          result.Append(problem.details);
        }
      }
      return result.ToString();
    }
  }

  [DataContract]
  class CompletenessList
  {
    [DataMember]
    public string baseFileName;
    [DataMember]
    public IList<AncestorLineInfo> limitList;


    public CompletenessList(string filename)
    {
      baseFileName = filename;
      limitList = new List<AncestorLineInfo>();
    }

  }

  [DataContract]
  public class HandledItem
  {
    private static readonly TraceSource trace = new("Sanity:HandledItem", SourceLevels.Warning);
    [DataMember]
    public string xref;
    [DataMember]
    public int number;
    [DataMember]
    public IList<RelationStack> relationStackList;

    public HandledItem(string xref = null, RelationStack relationStack = null)
    {
      this.xref = xref;
      number = 1;
      this.relationStackList = new List<RelationStack>();
      this.relationStackList.Add(relationStack);
    }
    public void Add(RelationStack relationStack)
    {
      trace.TraceInformation("Add to existing root person:" + number);
      //bool duplicateExists = false;
      foreach (RelationStack stack in relationStackList)
      {
        bool duplicate = false;
        if (stack.Count == relationStack.Count)
        {
          duplicate = true;
          for (int i = 0; (i < stack.Count) && duplicate; i++)
          {
            if (stack[i].personXref != relationStack[i].personXref)
            {
              duplicate = false;
            }
          }
        }
        if (duplicate)
        {
          //duplicateExists = true;
          //trace.TraceEvent(TraceEventType.Error, 0, "Error: Add to existing root person:Duplication!");
          //trace.TraceEvent(TraceEventType.Error, 0, stack.ToString());
          //trace.TraceEvent(TraceEventType.Error, 0, relationStack.ToString());
          // Just ignore duplicates for now
          return;
        }
      }
      this.relationStackList.Add(relationStack);
      foreach (RelationStack stack in relationStackList)
      {
        trace.TraceInformation(stack.ToString());
      }
      number++;
    }
  }

  public delegate void AncestorUpdate(AncestorLineInfo ancestor);

  [DataContract]
  public class AncestorStatistics
  {
    private static readonly TraceSource trace = new("Sanity:AncestorStatistics", SourceLevels.Warning);
    [DataMember]
    private readonly IDictionary<string, AncestorLineInfo> ancestorList;
    private IFamilyTreeStoreBaseClass familyTree;
    //[DataMember]
    //private int people;//, duplicatePeople;
    //[DataMember]
    //private int families, duplicateFamilies;
    //[DataMember]
    //private IList<string> analysedFamilies;
    [DataMember]
    private readonly IList<string> sanityCheckedFamilies;
    [DataMember]
    private readonly IList<string> analysedPeople;
    //[DataMember]
    //private readonly IList<HandledItem> analysedFamiliesNo;
    [DataMember]
    private readonly IList<HandledItem> analysedPeopleNo;
    [DataMember]
    private readonly int descendantGenerationNo;
    //private SearchMode mode;
    //private double progress;
    private readonly DateTime startTime;
    private DateTime endTime;
    /*private TimeSpan oldestParent;
    private TimeSpan youngestParent;
    private TimeSpan youngestAtEvent;
    private TimeSpan oldestAtEvent;*/
    //private int maxNoOfChildren;
    //private int maxNoOfParents;
    [DataMember]
    private readonly SanityCheckLimits limits;
    [DataMember]
    private readonly int ancestorGenerationNo;
    private readonly IProgressReporterInterface progressReporter;
    private double latestPercent;
    readonly AncestorUpdate updateCallback;

    RelationStack thisRelationStack;
    int thisGenerations;

    public const int DistantRecentLimit = 3;
    public const int AllGenerations = 1000;
    private bool StopRequested = false;

    public AncestorStatistics(IFamilyTreeStoreBaseClass familyTree, SanityCheckLimits limits, IProgressReporterInterface progressReporter = null, AncestorUpdate updateCallback = null)
    {
      this.familyTree = familyTree;
      this.descendantGenerationNo = limits.generationsForward;
      //this.mode = mode;
      ancestorGenerationNo = limits.generationsBack;

      //analysedFamilies = new List<string>();
      sanityCheckedFamilies = new List<string>();
      analysedPeople = new List<string>();
      //analysedFamiliesNo = new List<HandledItem>();
      analysedPeopleNo = new List<HandledItem>();
      this.progressReporter = progressReporter;
      this.updateCallback = updateCallback;
      StopRequested = false;

      this.limits = limits;
      latestPercent = 0.0;

      ancestorList = new Dictionary<string, AncestorLineInfo>();

      //people = 0;
      //families = 0;
      //duplicatePeople = 0;
      //duplicateFamilies = 0;
      //progress = 0.0;
      startTime = DateTime.Now;
      //maxNoOfChildren = 0;
      //maxNoOfParents = 0;
      /*youngestParent = TimeSpan.FromDays(100000);
      youngestAtEvent= TimeSpan.FromDays(100000);
      oldestParent = TimeSpan.FromDays(0);
      oldestAtEvent = TimeSpan.FromDays(0);*/

      trace.TraceData(TraceEventType.Information, 0, "Analysis of " + ancestorGenerationNo + " / " + descendantGenerationNo + "started at " + startTime);


    }

    /*public SearchMode GetMode()
    {
      return mode;
    }*/

    public int GetAncestorGenerationNo()
    {
      return ancestorGenerationNo;
    }
    public int GetDescendantGenerationNo()
    {
      return descendantGenerationNo;
    }
    public bool Stopped()
    {
      return StopRequested;
    }

    public IFamilyTreeStoreBaseClass GetFamilyTree()
    {
      return familyTree;
    }
    public void SetFamilyTree(IFamilyTreeStoreBaseClass tree)
    {
      familyTree = tree;
    }

    private int ToYears(TimeSpan difference)
    {
      return (int)(difference.Days / 365.25);
    }
    private int ToMonths(TimeSpan difference)
    {
      return (int)(difference.Days / (365.25 / 12));
    }

    bool IsInList(string person)
    {
      return ancestorList.ContainsKey(person);
    }

    public AncestorLineInfo GetAncestor(string person)
    {
      if (ancestorList.ContainsKey(person))
      {
        return ancestorList[person];
      }
      return null;
    }

    private string GetSexString(IndividualClass.IndividualSexType sex)
    {
      switch (sex)
      {
        case IndividualClass.IndividualSexType.Unknown:
          return "Unknown";
        case IndividualClass.IndividualSexType.Female:
          return "Woman";
        case IndividualClass.IndividualSexType.Male:
          return "Man";
      }
      return "";
    }

    public void AddToList(IndividualClass person, RelationStack relationStack, int depth, SanityCheckLimits.SanityProblemId id, string description, string duplicateUrl = null)
    {
      string url = "";
      IList<string> urlList = person.GetUrlList();
      if (urlList.Count > 0)
      {
        url = urlList[0];
      }
      AddToList(person.GetXrefName(),
                person.GetName(),
                url,
                GetSexString(person.GetSex()),
                AncestorStatistics.GetEventDateString(person, IndividualEventClass.EventType.Birth),
                AncestorStatistics.GetEventDateString(person, IndividualEventClass.EventType.Death),
                relationStack,
                depth,
                id,
                description, duplicateUrl);

    }

    public void AddToList(string rootAncestor, string name, string url, string sex, string birth, string death, RelationStack relationStack, int depth, SanityCheckLimits.SanityProblemId id, string description, string duplicateUrl = null)
    {
      trace.TraceInformation("AddToList(" + rootAncestor + "," + depth + "," + description + ")");
      if (relationStack != null)
      {
        trace.TraceInformation(relationStack.ToString(false));
      }

      if (ancestorList.ContainsKey(rootAncestor))
      {
        /*if (ancestorList[rootAncestor].details.IndexOf(description) < 0)
        {
          ancestorList[rootAncestor].details += "; " + description;
        }*/
        ancestorList[rootAncestor].problemList.Add(new SanityProblem(id, description, duplicateUrl));
        if (duplicateUrl != null)
        {
          ancestorList[rootAncestor].duplicate.Add(duplicateUrl);
        }

        if (this.updateCallback != null)
        {
          this.updateCallback(ancestorList[rootAncestor]);
        }

        return;
      }

      AncestorLineInfo newInfo = new(rootAncestor, name, url, sex, birth, death, relationStack, depth, id, description, duplicateUrl);

      if (this.updateCallback != null)
      {
        this.updateCallback(newInfo);
      }


      ancestorList.Add(rootAncestor, newInfo);
    }

    private string AppendStr(string str1, string str2, string glue)
    {
      if ((str1 != null) && (str1.Length > 0))
      {
        if ((str2 != null) && (str2.Length > 0))
        {
          return str1 + glue + str2;
        }
        return str1;
      }
      if ((str2 != null) && (str2.Length > 0))
      {
        return str2;
      }
      return "";
    }

    bool SearchKeyword(IndividualClass person, string keyword)
    {
      IList<NoteClass> notes = person.GetNoteList();
      IList<string> strings = keyword.Split(';');

      if (notes != null)
      {
        foreach (NoteClass note in notes)
        {
          if (note.note != null)
          {
            foreach (string key in strings)
            {
              if (note.note.ToLower().IndexOf(key.ToLower()) >= 0)
              {
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    private string GetPlaceStr(IndividualEventClass ev)
    {
      if (ev != null)
      {
        AddressClass address = ev.GetAddress();
        string returnStr = "";
        if (address != null)
        {
          string addressStr = address.ToString();

          if (addressStr != null)
          {
            returnStr += addressStr;
          }
        }
        PlaceStructureClass place = ev.GetPlace();
        if (place != null)
        {
          string placeStr = place.GetPlace();

          if (placeStr != null)
          {
            returnStr = AppendStr(placeStr, returnStr, ", ");
          }
        }
        return returnStr;
      }
      return "";
    }

    private void CheckRelatedFamilies(IndividualClass individual)
    {
      trace.TraceInformation("CheckRelatedFamilies(" + individual.GetName() + ")");

      foreach (FamilyXrefClass familyXref in individual.GetFamilySpouseList())
      {
        FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());
        if (family != null)
        {
          bool personFound = false;
          foreach (IndividualXrefClass parent in family.GetParentList())
          {
            if (parent.GetXrefName() == individual.GetXrefName())
            {
              personFound = true;
              break;
            }
          }
          if (!personFound)
          {
            trace.TraceData(TraceEventType.Warning, 0, "Could not find person reference in parent family " + individual.GetName() + " " + familyXref.GetXrefName());
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Could not find spouse family " + individual.GetName() + " " + familyXref.GetXrefName());
        }
      }

      foreach (FamilyXrefClass familyXref in individual.GetFamilyChildList())
      {
        FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());
        if (family != null)
        {
          bool personFound = false;
          foreach (IndividualXrefClass child in family.GetChildList())
          {
            if (child.GetXrefName() == individual.GetXrefName())
            {
              personFound = true;
              break;
            }
          }
          if (!personFound)
          {
            trace.TraceData(TraceEventType.Warning, 0, "Could not find person reference in child family " + individual.GetName() + " " + familyXref.GetXrefName());
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Could not find child family " + individual.GetName() + " " + familyXref.GetXrefName());
        }
      }
    }

    private void CheckRelatedIndividuals(FamilyClass family)
    {
      trace.TraceInformation("CheckRelatedIndividuals(" + family.GetXrefName() + ")");

      foreach (IndividualXrefClass individualXref in family.GetParentList())
      {
        IndividualClass parent = familyTree.GetIndividual(individualXref.GetXrefName());
        if (parent != null)
        {
          bool familyFound = false;
          foreach (FamilyXrefClass parentFamily in parent.GetFamilySpouseList())
          {
            if (parentFamily.GetXrefName() == family.GetXrefName())
            {
              familyFound = true;
              break;
            }
          }
          if (!familyFound)
          {
            trace.TraceData(TraceEventType.Information, 0, "Could not find family reference in parent  " + family.GetXrefName() + " " + individualXref.GetXrefName());
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Could not find parent individual " + family.GetXrefName() + " " + individualXref.GetXrefName());
        }
      }

      foreach (IndividualXrefClass individualXref in family.GetChildList())
      {
        IndividualClass child = familyTree.GetIndividual(individualXref.GetXrefName());
        if (child != null)
        {
          bool familyFound = false;
          foreach (FamilyXrefClass childFamily in child.GetFamilyChildList())
          {
            if (childFamily.GetXrefName() == family.GetXrefName())
            {
              familyFound = true;
              break;
            }
          }
          if (!familyFound)
          {
            trace.TraceData(TraceEventType.Information, 0, "Could not find family reference in child  " + family.GetXrefName() + " " + individualXref.GetXrefName());
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Could not find child individual " + family.GetXrefName() + " " + individualXref.GetXrefName());
        }
      }
    }

    private bool CheckValidMapPlace(IndividualEventClass ev)
    {
      if (ev != null)
      {
        PlaceStructureClass place = ev.GetPlace();
        if (place != null)
        {
          MapPosition mapPlace = place.GetMapPosition();

          return mapPlace != null;
        }
      }
      return false;
    }

    private void SanityCheckIndividual(IndividualClass person, RelationStack relationStack, int depth)
    {
      trace.TraceInformation("SanityCheckIndividual(" + person.GetName() + ")");
      IList<IndividualEventClass> evList = person.GetEventList();
      IndividualEventClass birth = person.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass death = person.GetEvent(IndividualEventClass.EventType.Death);
      IndividualClass.IndividualSexType sex = person.GetSex();

      if (person.GetName().Length <= 4)
      {
        trace.TraceInformation("Abort, short name (like N N) (" + person.GetName() + ")");
        return;
      }
      CheckRelatedFamilies(person);
      foreach (IndividualEventClass ev in evList)
      {
        if (ev.GetDate().CheckBadYear() || (ev.GetDate().GetDateType() == FamilyDateTimeClass.FamilyDateType.DateString))
        {
          trace.TraceInformation(" Bad date in event " + person.GetXrefName() + ":" + person.GetName() + " : " + ev.GetEventType() + " " + ev.GetDate());
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.eventLimitMin_e, "Bad year in  event: " + ev.GetEventType() + " " + ev.GetDate());
        }
        if (ev.badDate)
        {
          trace.TraceInformation(" Bad date-2 in event " + person.GetXrefName() + ":" + person.GetName() + " : " + ev.GetEventType() + " " + ev.GetDate());
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.eventLimitMin_e, "Bad year-2 in  event: " + ev.GetEventType() + " " + ev.GetDate());
        }
      }

      if ((birth != null) && (birth.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown))
      {
        if (limits.endYear.active && (birth.GetDate().ToDateTime().Year > limits.endYear.value) && (depth > DistantRecentLimit))
        {
          int relDepth = -1;
          if (relationStack != null)
          {
            RelationStack.RelDistance dist = relationStack.Distance();
            relDepth = dist.ancestorGen + dist.descendantGen + dist.marriageNo;
          }
          trace.TraceData(TraceEventType.Information, 0, "Abort, too distant, and recent person (" + person.GetName() + ", " + birth.GetDate() + ", depth " + depth + "/" + relDepth);
          return;
        }
        /*if (birth.GetDate().ToDateTime().Year < limits.endYear.value)
        {
          if (!person.GetPublic()) // vi måste gissa födelseåret....
          {
            trace.TraceInformation(" Private old person " + person.GetXrefName() + ":" + person.GetName() + ", b:" + birth.GetDate() + " " + person.GetEvent(IndividualEventClass.EventType.Birth));
            AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.oldPrivateProfile_e, "Old private profile event, born " + birth.GetDate());
          }
        }*/
        foreach (IndividualEventClass ev in evList)
        {
          if ((ev.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Unknown) &&
              (ev.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.DateString) &&
              (ev.GetEventType() != IndividualEventClass.EventType.Birth) &&
              (ev.GetEventType() != IndividualEventClass.EventType.RecordUpdate))
          {
            if ((ToYears(ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime()) < limits.eventLimitMin.value))
            {
              //youngestAtEvent = ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime();
              trace.TraceInformation(" Young person at event " + person.GetXrefName() + ":" + person.GetName() + ", b:" + birth.GetDate() + " " + person.GetEvent(IndividualEventClass.EventType.Birth) + " : " + ev.GetEventType() + " " + ev.GetDate());
              AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.eventLimitMin_e, "Young at event, born " + birth.GetDate() + " event" + ev.GetEventType() + " " + ev.GetDate());
            }
            if (ToYears(ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime()) > limits.eventLimitMax.value)
            {
              //oldestAtEvent = ev.GetDate().ToDateTime() - birth.GetDate().ToDateTime();
              trace.TraceInformation(" Old person at event " + person.GetXrefName() + ":" + person.GetName() + ", b:" + person.GetEvent(IndividualEventClass.EventType.Birth) + " : " + ev.GetEventType() + ev.GetDate());
              AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.eventLimitMax_e, "Old at event, born " + birth.GetDate() + " event" + ev.GetEventType() + " " + ev.GetDate());
            }
          }
        }
      }
      string birthDate = null;
      string deathDate = null;

      if (sex == IndividualClass.IndividualSexType.Unknown)
      {
        AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.unknownSex_e, "Unknown sex");
      }


      SanityCheckLimits.SanityProblemId birthEvType = SanityCheckLimits.SanityProblemId.unknownBirth_e;
      SanityCheckLimits.SanityProblemId deathEvType = SanityCheckLimits.SanityProblemId.unknownDeath_e;

      bool birthPlaceKnown = GetPlaceStr(birth).Length > 15;
      bool deathPlaceKnown = GetPlaceStr(death).Length > 15;

      bool birthMapPlaceKnown = CheckValidMapPlace(birth);
      bool deathMapPlaceKnown = CheckValidMapPlace(death);

      if ((birth != null) && birth.GetDate().ValidDate())
      {
        switch (birth.GetDate().GetDateType())
        {
          case FamilyDateTimeClass.FamilyDateType.Unknown:
            birthDate = "Unknown";
            birthEvType = SanityCheckLimits.SanityProblemId.unknownBirth_e;
            break;
          case FamilyDateTimeClass.FamilyDateType.Year:
          case FamilyDateTimeClass.FamilyDateType.YearMonth:
            birthDate = "Inexact";
            birthEvType = SanityCheckLimits.SanityProblemId.inexactBirthDeath_e;
            break;
          case FamilyDateTimeClass.FamilyDateType.DateString:
            birthDate = "Badly formed";
            birthEvType = SanityCheckLimits.SanityProblemId.inexactBirthDeath_e;
            break;
          default:
            break;
        }
        if ((death != null) && death.GetDate().ValidDate())
        {
          int personAge = ToYears(death.GetDate().ToDateTime() - birth.GetDate().ToDateTime());
          int ageToday = ToYears(DateTime.Now - birth.GetDate().ToDateTime());
          int minAge = limits.missingPartner.value;

          if ((personAge >= 40) && (ageToday >= minAge))
          {
            IList<FamilyXrefClass> spouseList = person.GetFamilySpouseList();

            if ((spouseList == null) || (spouseList.Count == 0))
            {
              string append = "";
              SanityCheckLimits.SanityProblemId evType = SanityCheckLimits.SanityProblemId.missingPartner_e;

              if (SearchKeyword(person, "ogift;unmarried"))
              {
                append = " (Note: Unmarried)";
                evType = SanityCheckLimits.SanityProblemId.missingPartnerMitigated_e;
              }
              AddToList(person, relationStack, depth, evType, "Person age " + personAge + " without partner" + append);
            }
          }
        }
      }
      else
      {
        birthDate = "Unknown";
        birthEvType = SanityCheckLimits.SanityProblemId.unknownBirth_e;
      }

      bool checkEmigration = false;
      //if (birth == null || birth.GetDate().ValidDate())
      {
        if ((death != null) && death.GetDate().ValidDate())
        {
          switch (death.GetDate().GetDateType())
          {
            case FamilyDateTimeClass.FamilyDateType.Unknown:
              deathDate = "Unknown";
              checkEmigration = true;
              deathEvType = SanityCheckLimits.SanityProblemId.unknownDeath_e;
              break;
            case FamilyDateTimeClass.FamilyDateType.Year:
            case FamilyDateTimeClass.FamilyDateType.YearMonth:
              deathDate = "Inexact";
              deathEvType = SanityCheckLimits.SanityProblemId.inexactBirthDeath_e;
              break;
            case FamilyDateTimeClass.FamilyDateType.DateString:
              deathDate = "Badly formed";
              deathEvType = SanityCheckLimits.SanityProblemId.inexactBirthDeath_e;
              break;
            default:
              break;
          }
        }
        else if (person.GetIsAlive() != IndividualClass.Alive.Yes)
        {
          deathDate = "Unknown";
          deathEvType = SanityCheckLimits.SanityProblemId.unknownDeath_e;
          checkEmigration = true;
        }
      }

      if (birthDate != null)
      {
        AddToList(person, relationStack, depth, birthEvType, birthDate + " birth date");
      }
      else
      {
        if (!birthPlaceKnown)
        {
          trace.TraceData(TraceEventType.Information, 0, " Short birth place " + person.GetXrefName() + ":" + person.GetName() + ": " + GetPlaceStr(birth));
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.shortAddress_e, "Short birth place (" + GetPlaceStr(birth).Trim() + ")");
        }
        if (!birthMapPlaceKnown)
        {
          trace.TraceData(TraceEventType.Information, 0, " No birth GPS place " + person.GetXrefName() + ":" + person.GetName() + ": " + GetPlaceStr(birth));
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.unknownGpsPosition_e, "Unknown GPS birth place (" + GetPlaceStr(birth).Trim() + ")");
        }
      }
      if (deathDate != null)
      {
        string append = "";

        if (checkEmigration && SearchKeyword(person, "emigrera;emigrate"))
        {
          append = " (Note: Emigrated)";
          if (deathEvType == SanityCheckLimits.SanityProblemId.unknownDeath_e)
          {
            deathEvType = SanityCheckLimits.SanityProblemId.unknownDeathEmigrated_e;
          }
        }
        AddToList(person, relationStack, depth, deathEvType, deathDate + " death date" + append);
      }
      else if (person.GetIsAlive() != IndividualClass.Alive.Yes)
      {
        if (!deathPlaceKnown)
        {
          trace.TraceData(TraceEventType.Information, 0, " Short death place " + person.GetXrefName() + ":" + person.GetName() + ": " + GetPlaceStr(death));
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.shortAddress_e, "Short death place (" + GetPlaceStr(death).Trim() + ")");
        }
        if (!deathMapPlaceKnown)
        {
          trace.TraceData(TraceEventType.Information, 0, " No death GPS place " + person.GetXrefName() + ":" + person.GetName() + ": " + GetPlaceStr(death));
          AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.unknownGpsPosition_e, "Unknown GPS death place (" + GetPlaceStr(death).Trim() + ")");
        }
      }

      if (limits.duplicateCheck.active)
      {
        thisRelationStack = relationStack;
        thisGenerations = depth;
        CompareTreeClass.SearchDuplicates(person, familyTree, familyTree, ReportMatchingProfiles, progressReporter);
      }

    }

    public bool AnalysePerson(string xref, RelationStack relationStack)
    {
      trace.TraceInformation("AnalysePerson (" + xref + ")");
      if (analysedPeople.Contains(xref))
      {
        trace.TraceInformation("  analyse " + xref + " done");
        //duplicatePeople++;
        if (relationStack != null)
        {
          analysedPeopleNo[analysedPeople.IndexOf(xref)].Add(relationStack);
        }

        /*if (descendantGenerationNo == 0)
        {
          return false;
        }*/
        //return true; // With the current implementation we will check people twice, but we need to do so to be able to check both ancestors and descendants.
        return false;
      }
      //this.people++;
      analysedPeople.Add(xref);
      if (relationStack != null)
      {
        analysedPeopleNo.Add(new HandledItem(xref, relationStack));
      }
      trace.TraceInformation("  analysed " + analysedPeople.Count + " people");
      return true;
    }
    public class ParentInfo
    {
      public DateTime birth;
      public DateTime death;
      public IndividualClass person;

      public ParentInfo()
      {
        birth = DateTime.MinValue;
        death = DateTime.MinValue;
        person = null;
      }
    }

    Relation.Type FindRelation(FamilyClass family, string lastPerson, string currentPerson)
    {
      Relation.Type lastRel = Relation.Type.Unknown;
      Relation.Type currRel = Relation.Type.Unknown;

      if (lastPerson == currentPerson)
      {
        return Relation.Type.Same;
      }
      foreach (IndividualXrefClass spouse in family.GetParentList())
      {
        string xref = spouse.GetXrefName();
        if (xref == lastPerson)
        {
          lastRel = Relation.Type.Parent;
        }
        if (xref == currentPerson)
        {
          currRel = Relation.Type.Parent;
        }
      }
      foreach (IndividualXrefClass child in family.GetChildList())
      {
        string xref = child.GetXrefName();
        if (xref == lastPerson)
        {
          lastRel = Relation.Type.Child;
        }
        if (xref == currentPerson)
        {
          currRel = Relation.Type.Child;
        }
      }
      if ((currRel == Relation.Type.Parent) && (lastRel == Relation.Type.Parent))
      {
        return Relation.Type.Spouse;
      }
      if ((currRel == Relation.Type.Child) && (lastRel == Relation.Type.Child))
      {
        return Relation.Type.Sibling;
      }
      if ((currRel == Relation.Type.Parent) && (lastRel == Relation.Type.Child))
      {
        return Relation.Type.Parent;
      }
      if ((currRel == Relation.Type.Child) && (lastRel == Relation.Type.Parent))
      {
        return Relation.Type.Child;
      }
      return Relation.Type.Unknown;

    }

    void CheckAndAddRelation(ref RelationStack stack, FamilyClass family, IndividualClass person)
    {
      if (stack != null)
      {
        Relation.Type relation = FindRelation(family, stack.GetLast(), person.GetXrefName());
        if (relation != Relation.Type.Same)
        {
          stack.Add(Relation.MakeRelation(person, relation));
        }
      }
    }

    RelationStack CopyStackAndAddPerson(RelationStack stack, FamilyClass family, IndividualClass person)
    {
      if (stack != null)
      {
        Debug.Assert(family != null);
        Debug.Assert(person != null);
        RelationStack newStack = stack.Duplicate();
        CheckAndAddRelation(ref newStack, family, person);
        return newStack;
      }
      return null;
    }

    private string DaysToString(int days)
    {
      if (days < 30)
      {
        return days + " days";
      }
      int months = days / 30;
      return months + " months and " + (days % 30) + " days";

    }

    public void SanityCheckFamily(FamilyClass family, RelationStack relationStack, int depth)
    {
      //DateTime oldestParentBirth = DateTime.MaxValue;
      //DateTime youngestParentBirth = DateTime.MinValue;
      /*DateTime motherBirth = DateTime.MinValue;
      DateTime motherDeath = DateTime.MinValue;
      DateTime fatherBirth = DateTime.MinValue;
      IndividualClass mother = null;
      IndividualClass father = null;*/

      if (family == null)
      {
        trace.TraceData(TraceEventType.Error, 0, " SanityCheckFamily Error: family == null");
        return;
      }
      trace.TraceInformation(" SanityCheckFamily " + family.GetXrefName());

      if (sanityCheckedFamilies.Contains(family.GetXrefName()))
      {
        return;
      }
      sanityCheckedFamilies.Add(family.GetXrefName());
      CheckRelatedIndividuals(family);
      ParentInfo mother = new();
      ParentInfo father = new();
      string sampleChildName = null;
      IndividualEventClass marriage = family.GetEvent(IndividualEventClass.EventType.FamMarriage);
      IList<IndividualEventClass> evList = family.GetEventList();
      int noOfParents = 0;
      int noOfChildren = 0;

      if ((marriage != null) && ((marriage.GetDate() == null) || !marriage.GetDate().ValidDate()))
      {
        marriage = null;
      }

      IndividualClass parentRef = null;
      {
        IList<IndividualXrefClass> parentList = family.GetParentList();
        //bool fewParentsWarned = false;
        if ((parentList != null) && (parentList.Count > 0))
        {
          foreach (IndividualXrefClass parentXref in parentList)
          {
            IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
            if (parent != null)
            {
              IndividualEventClass birth = parent.GetEvent(IndividualEventClass.EventType.Birth);
              IndividualEventClass death = parent.GetEvent(IndividualEventClass.EventType.Death);

              parentRef = parent;
              noOfParents++;

              if ((birth != null) && birth.GetDate().ValidDate() && (depth > DistantRecentLimit))
              {
                if (limits.endYear.active && (birth.GetDate().ToDateTime().Year > limits.endYear.value))
                {
                  trace.TraceInformation(" Abort distant, recent family" + family.GetXrefName());
                  return;
                }
              }

              if (parent.GetSex() == IndividualClass.IndividualSexType.Female)
              {
                if (mother.person != null)
                {
                  RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);
                  AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "More than one mother in family");
                }
                mother.person = parent;
                if ((birth != null) && birth.GetDate().ValidDate())
                {
                  mother.birth = birth.GetDate().ToDateTime();
                }
                if ((death != null) && death.GetDate().ValidDate())
                {
                  mother.death = death.GetDate().ToDateTime();
                }
              }
              else if (parent.GetSex() == IndividualClass.IndividualSexType.Male)
              {
                if (father.person != null)
                {
                  RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);
                  AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "More than one father in family");
                }
                father.person = parent;
                if ((birth != null) && birth.GetDate().ValidDate())
                {
                  father.birth = birth.GetDate().ToDateTime();
                }
                if ((death != null) && death.GetDate().ValidDate())
                {
                  father.death = death.GetDate().ToDateTime();
                }
              }
              if ((birth != null) && birth.GetDate().ValidDate())
              {
                if ((marriage != null) && marriage.GetDate().ValidDate())
                {
                  int ageAtMarriage = ToYears(marriage.GetDate().ToDateTime() - birth.GetDate().ToDateTime());
                  if (ageAtMarriage < 16)
                  {
                    RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);

                    AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.marriageProblem_e, "Spouse only " + ageAtMarriage + " years old at marriage");
                  }
                }
                if ((death != null) && death.GetDate().ValidDate())
                {
                  if ((marriage != null) && marriage.GetDate().ValidDate())
                  {
                    if (ToYears(death.GetDate().ToDateTime() - marriage.GetDate().ToDateTime()) < 0)
                    {
                      RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);
                      AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.marriageProblem_e, "Marriage after death");
                    }
                  }
                }
              }
            }
          }
          if ((marriage == null) && (mother.person != null) && (father.person != null))
          {
            if (father.person.GetName().Length > 5)
            {
              trace.TraceInformation(" Missing marriage date " + family.GetXrefName());
              RelationStack stack = CopyStackAndAddPerson(relationStack, family, mother.person);
              AddToList(mother.person, stack, depth, SanityCheckLimits.SanityProblemId.missingWeddingDate_e,
                 "Missing marriage date between " + father.person.GetName() + " and " + mother.person.GetName());
            }
          }
        }
      }
      /*if (maxNoOfParents < parentList.Count)
      {
        maxNoOfParents = parentList.Count;
      }*/
      //if ((mother.birth != null)  || (father.birth != null))
      {
        IList<IndividualXrefClass> childList = family.GetChildList();
        if (childList != null)
        {
          IList<DateTime> birthDateList = new List<DateTime>();
          foreach (IndividualXrefClass childXref in childList)
          {
            IndividualClass child = familyTree.GetIndividual(childXref.GetXrefName());

            if (child != null)
            {
              IndividualEventClass birth = child.GetEvent(IndividualEventClass.EventType.Birth);

              noOfChildren++;

              if (noOfParents == 0)
              {
                RelationStack stack = CopyStackAndAddPerson(relationStack, family, child);
                AddToList(child, stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "Family has no parents");
                //missingParents = false;
                noOfParents = -1;
              }
              if (sampleChildName == null)
              {
                sampleChildName = child.GetName();
              }

              /*if (!fewParentsWarned && ((parentList == null) || (parentList.Count < 2)))
              {
                int parentNo = 0;
                if(parentList != null)
                {
                  parentNo = parentList.Count;
                }
                RelationStack stack = CopyStackAndAddPerson(relationStack, family, child);
                AddToList(child, stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "Few parents: " + parentList.Count);
                fewParentsWarned = true;
              }*/
              if (birth != null)
              {
                if (birth.GetDate().ValidDate() && (childXref.GetPedigreeType() == PedigreeType.Birth))
                {
                  if ((marriage != null) && marriage.GetDate().ValidDate())
                  {
                    DateTime birthTime = birth.GetDate().ToDateTime();
                    DateTime marriageDate = marriage.GetDate().ToDateTime();
                    int yearsAfterMarriage = ToYears(birthTime - marriageDate);
                    if (yearsAfterMarriage < 0)
                    {
                      RelationStack stack = CopyStackAndAddPerson(relationStack, family, child);
                      AddToList(child, stack, depth + 1, SanityCheckLimits.SanityProblemId.parentLimitMin_e, "Child born in " + birthTime.Year + ", " + -yearsAfterMarriage + " years before marriage in " + marriageDate.Year);
                    }
                  }
                  // Only compare those where we know at least birth month for close births...
                  if (birth.GetDate().GetDateType() != FamilyDateTimeClass.FamilyDateType.Year)
                  {
                    birthDateList.Add(birth.GetDate().ToDateTime());
                  }
                  if ((mother.death != DateTime.MinValue) && (ToYears(mother.death - birth.GetDate().ToDateTime()) < 0))
                  {
                    //youngestParent = age;
                    trace.TraceInformation(" Mother dead when child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " was born at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, mother.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.motherLimitMax_e, "Mother died " + ToYears(birth.GetDate().ToDateTime() - mother.death) + " years before birth.");
                  }
                  else if ((mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) > limits.motherLimitMax.value))
                  {
                    //oldestParent = age;
                    trace.TraceInformation(" Old mother to child " + child.GetXrefName() + ":" + child.GetName() + " born " + birth.GetDate() + " mother born " + mother.birth.ToShortDateString());
                    //sanity.OldParent = true;
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, mother.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.motherLimitMax_e, "Old mother: " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years at birth");

                  }
                  if ((mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) < 0))
                  {
                    //youngestParent = age;
                    trace.TraceInformation(" Mother younger than child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, mother.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.parentLimitMin_e, "Child born " + ToYears(mother.birth - birth.GetDate().ToDateTime()) + " years before mother");
                  }
                  else if ((mother.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - mother.birth) < limits.parentLimitMin.value))
                  {
                    //youngestParent = age;
                    trace.TraceInformation(" Young mother to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, mother.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.parentLimitMin_e, "Young mother: " + ToYears(birth.GetDate().ToDateTime() - mother.birth) + " years at birth");
                  }
                  if ((father.death != DateTime.MinValue) && (ToMonths(father.death - birth.GetDate().ToDateTime()) < -8))
                  {
                    //youngestParent = age;
                    trace.TraceInformation(" Father dead when child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " was born at " + ToMonths(birth.GetDate().ToDateTime() - father.death) + " months");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, father.person), family, child);
                    //AddToList(child, stack, depth, "Father died at " + father.parent.GetEvent(IndividualEventClass.EventType.Death).ToString(false));
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.fatherLimitMax_e, "Father died " + ToMonths(birth.GetDate().ToDateTime() - father.death) + " months before birth.");
                  }
                  else if ((father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) > limits.fatherLimitMax.value))
                  {
                    //oldestParent = age;
                    trace.TraceInformation(" Old father to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " = " + father.birth.ToShortDateString() + " parent birth:");
                    //sanity.OldParent = true;
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, father.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.fatherLimitMax_e, "Old father: " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years at birth.");

                  }
                  if ((father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) < 0))
                  {
                    //youngestParent = age;
                    trace.TraceInformation("Father younger than child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, father.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.parentLimitMin_e, "Child born " + ToYears(father.birth - birth.GetDate().ToDateTime()) + " years before father");
                  }
                  else if ((father.birth != DateTime.MinValue) && (ToYears(birth.GetDate().ToDateTime() - father.birth) < limits.parentLimitMin.value))
                  {
                    //youngestParent = age;
                    trace.TraceInformation(" Young father to child " + child.GetXrefName() + ":" + child.GetName() + birth.GetDate() + " at " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years");
                    RelationStack stack = CopyStackAndAddPerson(CopyStackAndAddPerson(relationStack, family, father.person), family, child);
                    AddToList(child, stack, depth, SanityCheckLimits.SanityProblemId.parentLimitMin_e, "Young father: " + ToYears(birth.GetDate().ToDateTime() - father.birth) + " years at birth");
                  }
                }
              }
            }
          }
          //birthDateList.OrderBy((d1, d2) => DateTime.Compare(d1, d2));
          //ArrayList sorted = new ArrayList(birthDateList);
          ArrayList.Adapter((IList)birthDateList).Sort();

          DateTime lastBirth = DateTime.MinValue;

          IndividualClass parent = parentRef;
          if (mother.person != null)
          {
            parent = mother.person;
          }
          if (parent != null)
          {
            foreach (DateTime birth in birthDateList)
            {
              if (lastBirth != DateTime.MinValue)
              {
                if (birth.Subtract(lastBirth).Days <= 1)
                {
                  RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);
                  //stack.RemoveLast();
                  AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.twins_e, "Twins born at " + birth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                else if ((birth.Subtract(lastBirth).Days < limits.daysBetweenChildren.value))
                {
                  RelationStack stack = CopyStackAndAddPerson(relationStack, family, parent);
                  //stack.RemoveLast();

                  AddToList(parent, stack, depth + 1, SanityCheckLimits.SanityProblemId.daysBetweenChildren_e, "Close children found: Only " + DaysToString(birth.Subtract(lastBirth).Days) + " in between");
                }

              }
              lastBirth = birth;
            }
          }
          /* Remove loss of parents check here since it's also done in person check.
           * if ((noOfChildren > 0) && (noOfParents == 1))
          {
            if ((mother.person != null) && (father.person == null))
            {
              RelationStack stack = CopyStackAndAddPerson(relationStack, family, mother.person);
              AddToList(mother.person.GetXrefName(), stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "Unknown father to " + sampleChildName);
              //fewParentsWarned = true;
            }
            else if ((mother.person == null) && (father.person != null))
            {
              RelationStack stack = CopyStackAndAddPerson(relationStack, family, father.person);
              AddToList(father.person.GetXrefName(), stack, depth + 1, SanityCheckLimits.SanityProblemId.parentsProblem_e, "Unknown mother to " + sampleChildName);
              //fewParentsWarned = true;
            }
          }*/
          //sorted.Sort();
          //}
        }
      }
      if (noOfParents >= 1)
      {
        //string fatherExtension = "";
        string name = null;
        if (father.person != null)
        {
          name = father.person.GetName();
        }
        if (mother.person != null)
        {
          if (name != null)
          {
            name += " and " + mother.person.GetName();
          }
          else
          {
            name = mother.person.GetName();
          }
        }
        if (limits.noOfChildrenMax.value < noOfChildren)
        {
          //maxNoOfChildren = childList.Count;
          RelationStack stack = CopyStackAndAddPerson(relationStack, family, parentRef);
          AddToList(parentRef, stack, depth + 1, SanityCheckLimits.SanityProblemId.noOfChildrenMax_e, name + " has " + noOfChildren + " children");
        }
        if ((limits.noOfChildrenMin.value >= noOfChildren) && (noOfParents >= 2))
        {
          string childStr = "no children";
          string marriageExtension = "";
          double childrenPerYear = 0;
          int motherMarriedAtAge = 0;
          if (noOfChildren == 1)
          {
            childStr = "only one child";
          }
          else if (noOfChildren > 0)
          {
            childStr = "only " + noOfChildren + " children";
          }
          if ((marriage != null) && marriage.GetDate().ValidDate())
          {
            marriageExtension = ", married in " + marriage.GetDate().ToDateTime().Year.ToString();
            if (mother.birth != DateTime.MinValue)
            {
              motherMarriedAtAge = ToYears(marriage.GetDate().ToDateTime() - mother.birth);
              marriageExtension += ", at age " + motherMarriedAtAge;
            }
            if ((mother.death != DateTime.MinValue) || (father.death != DateTime.MinValue))
            {
              DateTime firstDeath = DateTime.MaxValue;
              if (mother.death != DateTime.MinValue)
              {
                firstDeath = mother.death;
              }
              if (father.death != DateTime.MinValue)
              {
                if (father.death < firstDeath)
                {
                  firstDeath = father.death;
                }
              }
              marriageExtension += ", before " + ToYears(firstDeath - marriage.GetDate().ToDateTime()) + " years together";
              if (ToYears(firstDeath - marriage.GetDate().ToDateTime()) == 0)
              {
                childrenPerYear = 1000;
              }
              else
              {
                childrenPerYear = (double)noOfChildren / (double)ToYears(firstDeath - marriage.GetDate().ToDateTime());
              }
            }
          }
          if ((marriage != null) && (marriage.GetDate().ToDateTime().Year < limits.endYear.value))
          {
            if ((childrenPerYear < 0.3) && (motherMarriedAtAge < 35))
            {
              //maxNoOfChildren = childList.Count;
              RelationStack stack = CopyStackAndAddPerson(relationStack, family, parentRef);
              AddToList(parentRef, stack, depth + 1, SanityCheckLimits.SanityProblemId.noOfChildrenMin_e, name + " had " + childStr + marriageExtension);
            }
          }

        }
      }
      foreach (IndividualEventClass ev in evList)
      {
        if (ev.GetDate().CheckBadYear() || (ev.GetDate().GetDateType() == FamilyDateTimeClass.FamilyDateType.DateString))
        {
          trace.TraceInformation(" Bad date in family event " + family.GetXrefName() + ":" + ev.ToString() + " " + ev.GetDate());
          if (parentRef != null)
          {
            AddToList(parentRef, relationStack, depth, SanityCheckLimits.SanityProblemId.eventLimitMin_e, "Bad year in family event: " + ev.GetEventType() + " " + ev.GetDate());
          }
        }
      }
    }

    /*public bool AnalyseFamily(string xref)
    {
      trace.TraceInformation("AnalyseFamily(" + xref + ")");
      if (analysedFamilies.Contains(xref))
      {
        trace.TraceInformation("  analyse " + xref + " done");
        duplicateFamilies++;
        analysedFamiliesNo[analysedFamilies.IndexOf(xref)].number++;

        if (descendantGenerationNo == 0)
        {
          return false;
        }
        return true;// With the current implementation we will check families twice, but we need to do so to be able to check both ancestors and descendants.
      }
      this.families++;
      analysedFamilies.Add(xref);
      analysedFamiliesNo.Add(new HandledItem(xref));
      trace.TraceInformation("  analysed " + families + " families");
      return true;
    }*/

    private int CountParents(IList<FamilyXrefClass> parentFamilyList)
    {
      int parentNo = 0;

      foreach (FamilyXrefClass xref in parentFamilyList)
      {
        FamilyClass family = familyTree.GetFamily(xref.GetXrefName());
        if (family != null)
        {
          IList<IndividualXrefClass> parentXrefList = family.GetParentList();
          if (parentXrefList != null)
          {
            parentNo += parentXrefList.Count;
          }
          else
          {
            trace.TraceEvent(TraceEventType.Information, 0, "Error: Family list " + xref.ToString() + " empty!");
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Error: Family " + xref.ToString() + " not found!");
        }
      }
      return parentNo;
    }

    private int CountChildren(IList<FamilyXrefClass> familyList)
    {
      int childNo = 0;

      foreach (FamilyXrefClass xref in familyList)
      {
        FamilyClass family = familyTree.GetFamily(xref.GetXrefName());
        if (family != null)
        {
          IList<IndividualXrefClass> childXrefList = family.GetChildList();
          if (childXrefList != null)
          {
            childNo += childXrefList.Count;
          }
          else
          {
            trace.TraceEvent(TraceEventType.Information, 0, "Error: Family list " + xref.ToString() + " empty!");
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Error: Family " + xref.ToString() + " not found!");
        }
      }
      return childNo;
    }

    private int CountSpouses(IList<FamilyXrefClass> familyList)
    {
      int spouseNo = 0;

      foreach (FamilyXrefClass xref in familyList)
      {
        FamilyClass family = familyTree.GetFamily(xref.GetXrefName());
        if (family != null)
        {
          IList<IndividualXrefClass> spouseXrefList = family.GetParentList();
          if (spouseXrefList != null)
          {
            spouseNo += spouseXrefList.Count;
          }
          else
          {
            trace.TraceEvent(TraceEventType.Information, 0, "Error: Family list " + xref.ToString() + " empty!");
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Error: Family " + xref.ToString() + " not found!");
        }
      }
      return spouseNo;
    }
    /*public bool CheckIfRootPerson(IndividualClass person)
    {
      IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();

      if ((parentFamilyList == null) || (parentFamilyList.Count == 0))
      {
        return true;
      }
      if (CountParents(parentFamilyList) < 1)
      {
        return true;
      }
      return false;
    }*/

    private int NumberOfParents(IList<FamilyXrefClass> parentFamilyList)
    {
      if ((parentFamilyList == null) || (parentFamilyList.Count == 0))
      {
        return 0;
      }
      return CountParents(parentFamilyList);
    }

    private int NumberOfChildren(IList<FamilyXrefClass> familyList)
    {
      if ((familyList == null) || (familyList.Count == 0))
      {
        return 0;
      }
      return CountChildren(familyList);
    }

    private int NumberOfSpouses(IList<FamilyXrefClass> familyList)
    {
      if ((familyList == null) || (familyList.Count == 0))
      {
        return 0;
      }
      return CountSpouses(familyList);
    }

    public IndividualClass.IndividualSexType GetSingleParentSex(IList<FamilyXrefClass> parentFamilyList)
    {
      //IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();
      IndividualClass.IndividualSexType sex = IndividualClass.IndividualSexType.Unknown;

      foreach (FamilyXrefClass xref in parentFamilyList)
      {
        FamilyClass family = familyTree.GetFamily(xref.GetXrefName());
        if (family != null)
        {
          IList<IndividualXrefClass> parentXrefList = family.GetParentList();
          foreach (IndividualXrefClass parentXref in parentXrefList)
          {
            IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());

            if (parent != null)
            {
              switch (parent.GetSex())
              {
                case IndividualClass.IndividualSexType.Female:
                  if (sex == IndividualClass.IndividualSexType.Unknown)
                  {
                    sex = IndividualClass.IndividualSexType.Female;
                  }
                  else
                  {
                    return IndividualClass.IndividualSexType.Unknown;
                  }
                  break;

                case IndividualClass.IndividualSexType.Male:
                  if (sex == IndividualClass.IndividualSexType.Unknown)
                  {
                    sex = IndividualClass.IndividualSexType.Male;
                  }
                  else
                  {
                    return IndividualClass.IndividualSexType.Unknown;
                  }
                  break;
              }
            }
          }
        }
      }
      return sex;
    }

    string Concat(string str, string newStr)
    {
      if (str.Length > 0)
      {
        return str + ", " + newStr;
      }
      return newStr;
    }

    PedigreeType GetPedigreeType(FamilyXrefClass familyXref, string individualXref)
    {
      if (familyXref.pedigreeType != PedigreeType.Unknown)
      {
        return familyXref.pedigreeType;
      }
      else
      {
        FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

        if (family != null)
        {
          IList<IndividualXrefClass> childXrefList = family.GetChildList();

          foreach (IndividualXrefClass child in childXrefList)
          {
            if (child.GetXrefName().Equals(individualXref))
            {
              return child.GetPedigreeType();
            }
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "Error: family " + familyXref.GetXrefName() + " not found");
        }
      }
      return PedigreeType.Unknown;
    }

    private void AnalysePersonWithFamilies(string xref, int depth = 0, RelationStack relationStack = null)
    {
      if (AnalysePerson(xref, relationStack))
      {
        IndividualClass person = familyTree.GetIndividual(xref);
        if (person != null)
        {
          SanityCheckIndividual(person, relationStack, depth);

          IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();
          IList<FamilyXrefClass> spouseFamilyList = person.GetFamilySpouseList();

          int noOfParents = NumberOfParents(parentFamilyList);
          int noOfChildren = NumberOfChildren(spouseFamilyList);

          if (person.GetPublic())
          {
            if ((noOfParents == 0) && (noOfChildren == 0))
            {
              int noOfSpouses = NumberOfSpouses(spouseFamilyList);
              AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsMissing_e, "No parents or children and " + noOfSpouses + " spouses");
            }
            else if (noOfParents == 0)
            {
              AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsMissing_e, "No parents");
            }
          }

          if (parentFamilyList != null)
          {
            if (noOfParents == 1)
            {
              IndividualClass.IndividualSexType parentSex = GetSingleParentSex(parentFamilyList);
              switch (parentSex)
              {
                case IndividualClass.IndividualSexType.Female:
                  {
                    string appended = "";

                    if (SearchKeyword(person, "oäkta;bastard"))
                    {
                      appended = " (Note: Bastard)";
                    }
                    AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsMissing_e, "Unknown father" + appended);
                  }
                  break;
                case IndividualClass.IndividualSexType.Male:
                  AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsMissing_e, "Unknown mother");
                  break;
                case IndividualClass.IndividualSexType.Unknown:
                  AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsMissing_e, "Only one parent");
                  break;
              }
              //}
            }
            else if ((noOfParents > 2) || (parentFamilyList.Count > 1))
            {
              string familyList = "";
              foreach (FamilyXrefClass familyXref in parentFamilyList)
              {
                PedigreeType pedigreeType = GetPedigreeType(familyXref, person.GetXrefName());
                switch (pedigreeType)
                {
                  case PedigreeType.Birth:
                    familyList = Concat(familyList, "biological");
                    break;
                  case PedigreeType.Foster:
                    familyList = Concat(familyList, "foster");
                    break;
                  case PedigreeType.Adopted:
                    familyList = Concat(familyList, "adopted");
                    break;
                  case PedigreeType.Sealing:
                    familyList = Concat(familyList, "sealing");
                    break;
                }
              }
              if (familyList.Length > 0)
              {
                familyList = " (" + familyList + ")";
              }
              AddToList(person, relationStack, depth, SanityCheckLimits.SanityProblemId.parentsProblem_e, noOfParents + " parents in " + parentFamilyList.Count + " families" + familyList);
            }
            /*if (parentFamilyList.Count > 1)
            {
              AddToList(person.ToString(), relationStack, depth, SanityCheckLimits.SanityProblemId.parentsProblem_e, "Child in " + parentFamilyList.Count + " families");
            }*/
            foreach (FamilyXrefClass familyXref in parentFamilyList)
            {
              FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

              SanityCheckFamily(family, relationStack, depth);
            }
          }

          if (spouseFamilyList != null)
          {
            foreach (FamilyXrefClass familyXref in spouseFamilyList)
            {
              FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

              if (family != null)
              {
                SanityCheckFamily(family, relationStack, depth);

                IList<IndividualXrefClass> spouseList = family.GetParentList();
                foreach (IndividualXrefClass spouseXref in spouseList)
                {
                  if (AnalysePerson(spouseXref.GetXrefName(), relationStack))
                  {
                    IndividualClass spouse = familyTree.GetIndividual(spouseXref.GetXrefName());

                    if (spouse != null)
                    {
                      CheckAndAddRelation(ref relationStack, family, spouse);
                      SanityCheckIndividual(spouse, relationStack, depth);
                    }
                  }
                }
              }
              else
              {
                trace.TraceData(TraceEventType.Error, 0, "Error: person:" + person.GetXrefName() + " " + person.GetName() + "'s spousefamily " + familyXref + " == null");
              }
            }
          }
        }
      }
    }




    public void AnalyseAncestors(IndividualClass person, int depth, double progress, RelationStack relationStack, Relation.Type relation = Relation.Type.Person)
    {
      trace.TraceInformation("AnalyseAncestors(" + person.GetName() + ")" + depth + " / " + ancestorGenerationNo);

      if (relationStack == null)
      {
        relationStack = new RelationStack();
        thisRelationStack = relationStack;
        relationStack.Add(Relation.MakeRelation(person, Relation.GetSex(person)));
      }
      else
      {
        relationStack.Add(Relation.MakeRelation(person, relation));
      }

      AnalysePersonWithFamilies(person.GetXrefName(), depth, relationStack);

      if (depth < ancestorGenerationNo)
      {
        IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();

        if (parentFamilyList != null)
        {
          int familyNo = 1;
          foreach (FamilyXrefClass familyXref in parentFamilyList)
          {
            FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

            if (family != null)
            {
              IList<IndividualXrefClass> parentXrefList = family.GetParentList();

              if (parentXrefList != null)
              {
                int parentNo = 0;
                foreach (IndividualXrefClass parentXref in parentXrefList)
                {
                  IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
                  if (parent != null)
                  {
                    double levelAddition = Math.Pow(2, -depth);
                    double progressPercent;

                    trace.TraceInformation("progress = " + progress.ToString() + " gen:" + depth + " => progress: " + progress + " + levelAdd:" + levelAddition + " * (famNo:" + familyNo + " / famCnt:" + parentFamilyList.Count + ") * (parNo:" + parentNo + " / parCnt:" + parentXrefList.Count + ") = " + (double)(progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count)) + " " + parentXref.GetXrefName() + "=" + parent.GetName().ToString());
                    progressPercent = progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count);
                    if (latestPercent > progressPercent)
                    {
                      trace.TraceData(TraceEventType.Warning, 0, "Progress = backwards!!! " + progressPercent.ToString("P2") + "<" + latestPercent.ToString("P2"));
                    }
                    latestPercent = progressPercent;
                    trace.TraceInformation("Progress = " + progressPercent.ToString("P2"));
                    if (progressReporter != null)
                    {
                      progressReporter.ReportProgress(progressPercent * 100.0, "Analyzing: " + analysedPeople.Count + " people and " + sanityCheckedFamilies.Count + " families. Found " + ancestorList.Count + " problems...");
                      if (progressReporter.CheckIfStopRequested())
                      {
                        trace.TraceEvent(TraceEventType.Warning, 0, "Stop requested!");
                        StopRequested = true;
                      }
                    }
                    if (!StopRequested)
                    {
                      RelationStack stack2 = relationStack.Duplicate();

                      AnalyseAncestors(parent, depth + 1, progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count), stack2, Relation.GetParentRelation(parent));
                    }
                  }
                  else
                  {
                    trace.TraceEvent(TraceEventType.Error, 0, "Error person " + parentXref.GetXrefName() + " not found in database!");
                  }
                  parentNo++;
                }
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
            }

          }
          familyNo++;
        }

        /*else
        {
          //AddToList(person.ToString(), relationStack, depth, SanityCheckLimits.SanityProblemId.generationlimited_e, "Max depth, " + NumberOfParents(person) + " parents");
        }*/
      }
      AnalyseDescendants(person, descendantGenerationNo, depth, progress, relationStack, Relation.Type.Person);
    }

    public static string GetEventDateString(IndividualClass person, IndividualEventClass.EventType evType)
    {
      if (person != null)
      {
        IndividualEventClass ev = person.GetEvent(evType);

        if (ev != null)
        {
          FamilyDateTimeClass date = ev.GetDate();

          if (date != null)
          {
            return date.ToString();
          }
        }
      }
      return "";
    }

    private void ReportMatchingProfiles(IFamilyTreeStoreBaseClass familyTree1, string person1, IFamilyTreeStoreBaseClass familyTree2, string person2)
    {
      IndividualClass person1full = familyTree1.GetIndividual(person1);
      IndividualClass person2full = familyTree2.GetIndividual(person2);

      if ((person1full != null) && (person2full != null))
      {
        StringBuilder builder = new();
        builder.Append("Possible duplicate profile: ");
        builder.Append(person2full.GetName());
        builder.Append(" (");
        builder.Append(GetEventDateString(person2full, IndividualEventClass.EventType.Birth));
        builder.Append(" - ");
        builder.Append(GetEventDateString(person2full, IndividualEventClass.EventType.Death));
        builder.Append(")");

        if (person1full.GetUrlList().Count > 0)
        {
          IList<string> urls1 = person1full.GetUrlList();
          int ix1 = urls1[0].LastIndexOf('/');
          string id1 = "";


          if (ix1 >= 0)
          {
            id1 = urls1[0].Substring(ix1 + 1);
          }

          foreach (string url in person2full.GetUrlList())
          {
            int ix2 = url.LastIndexOf('/');
            string id2 = "";
            if (ix2 >= 0)
            {
              id2 = url.Substring(ix2 + 1);
            }

            if (id1.Length > 0 && id2.Length > 0)
            {
              string CompareUrl = "https://www.geni.com/merge/compare/" + id1 + "?return=match%3B&to=" + id2;
              trace.TraceEvent(TraceEventType.Verbose, 0, "url " + CompareUrl);
              AddToList(person1full, thisRelationStack, thisGenerations, SanityCheckLimits.SanityProblemId.duplicateCheck_e, builder.ToString(), CompareUrl);
            }
            else
            {
              trace.TraceEvent(TraceEventType.Verbose, 0, "comp-url " + urls1[0] + " " + ix1 + " " + id1 + " " + url + " " + ix2 + " " + id2);
              AddToList(person1full, thisRelationStack, thisGenerations, SanityCheckLimits.SanityProblemId.duplicateCheck_e, builder.ToString(), url);
            }
          }
          if (person2full.GetUrlList().Count == 0)
          {
            AddToList(person1full, thisRelationStack, thisGenerations, SanityCheckLimits.SanityProblemId.duplicateCheck_e, builder.ToString());
          }
        }
        else
        {
          trace.TraceEvent(TraceEventType.Warning, 0, "no urls ");
        }
      }
    }

    public void AnalyseTree(IndividualClass person)
    {
      RelationStack stack = new();
      AnalyseAncestors(person, 0, 0.0, stack);
      endTime = DateTime.Now;
      /*if (limits.duplicateCheck.active)
      {
        CompareTreeClass.CompareTrees(familyTree, familyTree, ReportMatchingProfiles, progressReporter);
      }*/
      progressReporter.Completed();
    }


    public void CheckProfileList(List<string> xrefList)
    {
      trace.TraceInformation("CheckProfileList(" + xrefList.Count + ")");
      int counter = 0;
      foreach (string xref in xrefList)
      {
        trace.TraceInformation("checking profile " + xref + ", number " + counter + " of " + xrefList.Count + ")");
        AnalysePersonWithFamilies(xref);
        counter++;
        if (progressReporter != null)
        {
          progressReporter.ReportProgress(100.0 * counter / xrefList.Count, "Analyzing: " + counter + " profiles of " + xrefList.Count + " . Found " + ancestorList.Count + " problems...");
          if (progressReporter.CheckIfStopRequested())
          {
            break;
          }
        }
      }
      endTime = DateTime.Now;
      progressReporter.Completed();
    }



    public void AnalyseDescendants(IndividualClass person, int descendantDepth, int depth, double progress, RelationStack relationStack, Relation.Type relation)
    {
      trace.TraceInformation("AnalyseDescendants(" + person.GetName() + ")");
      if (relation != Relation.Type.Person)
      {
        relationStack.Add(Relation.MakeRelation(person, relation));
      }

      AnalysePersonWithFamilies(person.GetXrefName(), depth, relationStack);

      if (descendantDepth > 0)
      {
        IList<FamilyXrefClass> spouseList = person.GetFamilySpouseList();

        if (spouseList != null)
        {
          trace.TraceData(TraceEventType.Information, 0, "Depth:" + descendantDepth + ", Spouses: " + spouseList.Count);
          bool processChildren = true;

          if (limits.endYear.active)
          {
            foreach (FamilyXrefClass familyXref in spouseList)
            {
              trace.TraceInformation(" Descendant:person birth check (" + person.GetName() + ") family " + familyXref.ToString());
              FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());
              IList<IndividualXrefClass> parentXrefList = family.GetParentList();
              foreach (IndividualXrefClass parentXref in parentXrefList)
              {
                IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
                if ((parent != null) && processChildren)
                {
                  IndividualEventClass birth = parent.GetEvent(IndividualEventClass.EventType.Birth);

                  if ((birth != null) && (birth.GetDate() != null) && birth.GetDate().ValidDate())
                  {
                    DateTime birthDate = birth.GetDate().ToDateTime();

                    if (limits.endYear.value < birthDate.Year)
                    {
                      trace.TraceEvent(TraceEventType.Information, 0, "Stop processing due to endyear reached " + limits.endYear.value + " " + parent.GetName() + " " + birthDate.ToString());
                      processChildren = false;
                    }
                  }
                }
              }
            }
          }

          if (processChildren)
          {
            foreach (FamilyXrefClass familyXref in spouseList)
            {
              trace.TraceInformation(" Descendant:person (" + person.GetName() + ") family " + familyXref.ToString());
              FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

              if (family != null)
              {
                IList<IndividualXrefClass> childXrefList = family.GetChildList();

                if (childXrefList != null)
                {
                  int parentNo = 0;
                  foreach (IndividualXrefClass childXref in childXrefList)
                  {
                    IndividualClass child = familyTree.GetIndividual(childXref.GetXrefName());
                    if (child != null)
                    {
                      trace.TraceInformation(" Descendant:person (" + person.GetName() + ") child " + child.GetName());

                      trace.TraceInformation("Progress = " + progress.ToString("P2"));
                      if (progressReporter != null)
                      {
                        progressReporter.ReportProgress(latestPercent * 100.0, "Analyzing: " + analysedPeople.Count + " people and " + sanityCheckedFamilies.Count + " families. Found " + ancestorList.Count + " problems...");
                        if (progressReporter.CheckIfStopRequested())
                        {
                          trace.TraceEvent(TraceEventType.Warning, 0, "Stop requested!");
                          StopRequested = true;
                        }
                      }
                      if (!StopRequested)
                      {
                        RelationStack stack2 = relationStack.Duplicate();

                        AnalyseDescendants(child, descendantDepth - 1, depth + 1, progress, stack2, Relation.GetChildRelation(child));
                      }
                    }
                    else
                    {
                      trace.TraceEvent(TraceEventType.Error, 0, "Error person " + childXref.GetXrefName() + " not found in database!");
                    }
                    parentNo++;
                  }
                }
              }
              else
              {
                trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
              }
            }
          }
        }
        else
        {
          trace.TraceInformation(" person (" + person.GetName() + ") spouse family = null");
        }
      }

    }

    public void Print()
    {
      //ancestorList.OrderBy<int, depth>();

      trace.TraceInformation("Analysis started at " + startTime + " done at " + endTime);
      trace.TraceInformation("Ancestor overview:");
      trace.TraceInformation("  analysed " + analysedPeople.Count + " people   ");
      trace.TraceInformation("  analysed " + sanityCheckedFamilies.Count + " families ");
      trace.TraceInformation("  roots    " + ancestorList.Count);
      //trace.TraceInformation("  max children: " + maxNoOfChildren+ " parents " + maxNoOfParents);
      trace.TraceInformation(familyTree.GetShortTreeInfo());
      //familyTree.Print();

      {
        IEnumerable<AncestorLineInfo> query = ancestorList.Values.OrderBy(ancestor => ancestor.depth);

        trace.TraceInformation("Roots:");
        foreach (AncestorLineInfo root in query)
        {
          IndividualClass person = familyTree.GetIndividual(root.rootAncestor);
          if (person != null)
          {
            trace.TraceInformation("  " + root.depth + " generations: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death));
          }
        }
      }
      {
        IEnumerable<HandledItem> query = analysedPeopleNo.OrderByDescending(ancestor => ancestor.number);
        int i = 0;

        trace.TraceInformation("Multiply Referenced:");
        foreach (HandledItem item in query)
        {
          IndividualClass person = familyTree.GetIndividual(item.xref);
          if (person != null)
          {
            if (item.number > 1)
            {
              trace.TraceInformation("  Referenced " + item.number + " times: " + person.GetName() + " " + person.GetDate(IndividualEventClass.EventType.Birth) + " - " + person.GetDate(IndividualEventClass.EventType.Death));
            }
          }
          i++;
        }
        trace.TraceInformation("Items:" + i);
      }
    }

    public JobInfo GetJobInfo(int jobId)
    {
      JobInfo info = new();

      info.Profiles = analysedPeople.Count;
      info.Families = sanityCheckedFamilies.Count;
      info.IssueList = ancestorList.Values;
      info.StartTime = startTime;
      info.EndTime = endTime;
      info.JobId = jobId;

      return info;
    }

  }

  public class CheckRelation
  {
    private readonly IProgressReporterInterface progressReporter;
    private double latestPercent;
    private int totalGenerations;
    private bool StopRequested = false;
    private static readonly TraceSource trace = new("CheckRelation", SourceLevels.Warning);
    private bool IsSubsetOf(RelationStack smallStack, RelationStack bigStack)
    {
      foreach (Relation sRel in smallStack)
      {
        bool subset = false;
        foreach (Relation bRel in bigStack)
        {
          if (sRel.personXref == bRel.personXref)
          {
            subset = true;
          }
        }
        if (!subset)
        {
          return false;
        }
      }
      return true;

    }

    private string CalculateRelation(RelationStack person1, RelationStack person2)
    {
      string str = "";

      int minGenerations = Math.Min(person1.Count, person2.Count);
      int maxGenerations = Math.Max(person1.Count, person2.Count);
      int diffGenerations = maxGenerations - minGenerations;

      switch (minGenerations)
      {
        case 1:
          str = "the same person";
          break;
        case 2:
          str = "sibling";
          break;
        case 3:
          str = "cousin";
          break;
        case 4:
          str = "second cousin";
          break;
        case 5:
          str = "third cousin";
          break;
        case 6:
          str = "fourth cousin";
          break;
        default:
          str = (minGenerations - 2) + "-th cousin";
          break;
      }
      if (diffGenerations != 0)
      {
        switch (diffGenerations)
        {
          case 1:
            str += " once removed";
            break;
          case 2:
            str += " twice removed";
            break;
          default:
            str += " " + diffGenerations + " times removed";
            break;
        }
      }

      return str;
    }

    public Relation.Type InvertRelation(Relation.Type rel)
    {
      //Relation.Type newRel = Relation.Type.Person;
      switch (rel)
      {
        case Relation.Type.Parent:
        case Relation.Type.Person:
          return Relation.Type.Child;
        case Relation.Type.Father:
        case Relation.Type.Man:
          return Relation.Type.Son;
        case Relation.Type.Mother:
        case Relation.Type.Woman:
          return Relation.Type.Daughter;
        default:
          trace.TraceInformation("sex inversion problem!" + rel);
          return Relation.Type.Person;
      }

    }
    public Relation.Type GetSex(Relation.Type rel)
    {
      //Relation.Type newRel = Relation.Type.Person;
      switch (rel)
      {
        case Relation.Type.Parent:
        case Relation.Type.Child:
        case Relation.Type.Person:
          return Relation.Type.Person;
        case Relation.Type.Father:
        case Relation.Type.Son:
        case Relation.Type.Man:
          return Relation.Type.Man;
        case Relation.Type.Mother:
        case Relation.Type.Daughter:
        case Relation.Type.Woman:
          return Relation.Type.Woman;
        default:
          trace.TraceInformation("sex problem!" + rel);
          return Relation.Type.Person;
      }

    }

    public CheckRelation(IFamilyTreeStoreBaseClass familyTree, string xrefPerson1, string xrefPerson2, int noOfGenerations, ref RelationStackList relationList, IProgressReporterInterface progress)
    {
      if (familyTree != null)
      {
        IndividualClass person1 = familyTree.GetIndividual(xrefPerson1);
        IndividualClass person2 = familyTree.GetIndividual(xrefPerson2);

        relationList.sourceTree = familyTree.GetSourceFileName();
        relationList.time = DateTime.Now;

        progressReporter = progress;

        if ((person1 != null) && (person2 != null))
        {
          IList<RelationStack> person1verified = new List<RelationStack>();
          IList<RelationStack> person2verified = new List<RelationStack>();
          IDictionary<string, RelationStack> person1Ancestors = new Dictionary<string, RelationStack>();
          IDictionary<string, RelationStack> person2Ancestors = new Dictionary<string, RelationStack>();

          LoadAncestors(familyTree, person1, ref person1Ancestors, noOfGenerations, Relation.GetSex(person1), null, 0.0, progress, " Loading ancestors to " + person1.GetName().ToString() + " (1/2) ");
          LoadAncestors(familyTree, person2, ref person2Ancestors, noOfGenerations, Relation.GetSex(person2), null, 0.0, progress, " Loading ancestors to " + person2.GetName().ToString() + " (2/2) ");

          IEnumerator<KeyValuePair<string, RelationStack>> person1enum = person1Ancestors.GetEnumerator();

          while (person1enum.MoveNext())
          {
            IEnumerator<KeyValuePair<string, RelationStack>> person2enum = person2Ancestors.GetEnumerator();

            while (person2enum.MoveNext())
            {
              if (person1enum.Current.Key == person2enum.Current.Key)
              {
                bool duplicate = false;
                trace.TraceInformation("Found match!" + person1verified.Count);


                for (int i = 0; i < person1verified.Count; i++)
                {
                  RelationStack stack1 = person1verified[i];
                  RelationStack stack2 = person2verified[i];

                  if (IsSubsetOf(stack1, person1enum.Current.Value) && IsSubsetOf(stack2, person2enum.Current.Value))
                  {
                    duplicate = true;
                    trace.TraceInformation("Don't add this. Duplicate of number " + i);
                    trace.TraceInformation(person1enum.Current.Value.ToString(false));
                    trace.TraceInformation(person2enum.Current.Value.ToString(false));
                  }
                }
                if (!duplicate)
                {
                  trace.TraceInformation("Unique match, added!");
                  person1verified.Add(person1enum.Current.Value);
                  person2verified.Add(person2enum.Current.Value);

                  trace.TraceInformation(person1enum.Current.Value.ToString(false));
                  trace.TraceInformation(person2enum.Current.Value.ToString(false));
                }

              }
            }
          }
          trace.TraceInformation("Done searching! Found " + person1verified.Count + " matches!");
          for (int i = 0; i < person1verified.Count; i++)
          {
            trace.TraceInformation("Final match " + (i + 1) + " out of " + person1verified.Count + " distance " + person1verified[i].Count + " + " + person2verified[i].Count + " = " + (person1verified[i].Count + person2verified[i].Count) + " steps or " + CalculateRelation(person1verified[i], person2verified[i]));
            //trace.TraceInformation(person1verified[i].ToString(familyTree));
            //trace.TraceInformation(person2verified[i].ToString(familyTree));

            if ((person1verified[i].Count >= 1) && (person2verified[i].Count >= 1))
            {
              if (person1verified[i][person1verified[i].Count - 1].personXref == person2verified[i][person2verified[i].Count - 1].personXref)
              {
                RelationStack printStack = person1verified[i].Duplicate();

                if (person2verified[i].Count > 1)
                {
                  // Turn one of the relation stacks upside down to get kinship...
                  for (int j = (person2verified[i].Count - 2); j >= 0; j--)
                  {
                    printStack.Add(new Relation(InvertRelation(person2verified[i][j].type), person2verified[i][j].personXref, person2verified[i][j].name, person2verified[i][j].url, person2verified[i][j].birth, person2verified[i][j].death));
                  }
                }
                trace.TraceInformation(printStack.ToString(false));
                trace.TraceInformation(printStack.CalculateRelation(false));
                if (relationList != null)
                {
                  relationList.relations.Add(printStack.Duplicate());
                  trace.TraceInformation("Add:" + printStack.CalculateRelation(false));
                }
              }
              else
              {
                trace.TraceEvent(TraceEventType.Error, 0, "error: " + person1verified[i][person1verified[i].Count - 1].personXref + "!=" + person2verified[i][person2verified[i].Count - 1].personXref);
                trace.TraceEvent(TraceEventType.Error, 0, person1verified[i].ToString(false));
                trace.TraceEvent(TraceEventType.Error, 0, person2verified[i].ToString(false));
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "error: " + person1verified[i].Count + " or " + person2verified[i].Count + " < 2");
              trace.TraceEvent(TraceEventType.Error, 0, person1verified[i].ToString(false));
              trace.TraceEvent(TraceEventType.Error, 0, person2verified[i].ToString(false));
            }
          }
        }
      }
    }

    private Relation.Type GetParentRelation(IndividualClass person)
    {
      switch (person.GetSex())
      {
        case IndividualClass.IndividualSexType.Female:
          return Relation.Type.Mother;
        case IndividualClass.IndividualSexType.Male:
          return Relation.Type.Father;
        default:
          return Relation.Type.Parent;

      }
    }


    private void LoadAncestors(IFamilyTreeStoreBaseClass familyTree, IndividualClass person, ref IDictionary<string, RelationStack> ancestors, int generations, Relation.Type relation, RelationStack relationStack, double startProgress, IProgressReporterInterface progressReporter, string progressDescription)
    {
      double progress = startProgress;
      trace.TraceInformation("LoadAncestors(" + person.GetName() + "," + generations + "," + ancestors.Count + ")");

      if (relationStack == null)
      {
        relationStack = new RelationStack();
        relationStack.Add(Relation.MakeRelation(person, Relation.GetSex(person)));
        totalGenerations = generations;
      }
      else
      {
        relationStack.Add(Relation.MakeRelation(person, relation));
      }

      if (!ancestors.ContainsKey(person.GetXrefName()))
      {
        ancestors.Add(person.GetXrefName(), relationStack.Duplicate());
      }
      else
      {
        trace.TraceInformation("Add new relation to person!");
      }

      if (generations > 0)
      {
        IList<FamilyXrefClass> parentFamilyList = person.GetFamilyChildList();
        int depth = totalGenerations - generations;

        if (parentFamilyList != null)
        {
          int familyNo = 1;
          foreach (FamilyXrefClass familyXref in parentFamilyList)
          {
            FamilyClass family = familyTree.GetFamily(familyXref.GetXrefName());

            if (family != null)
            {
              IList<IndividualXrefClass> parentXrefList = family.GetParentList();

              if (parentXrefList != null)
              {
                int parentNo = 0;
                foreach (IndividualXrefClass parentXref in parentXrefList)
                {
                  IndividualClass parent = familyTree.GetIndividual(parentXref.GetXrefName());
                  if (parent != null)
                  {
                    RelationStack stack2 = relationStack.Duplicate();

                    double levelAddition = Math.Pow(2, -depth);

                    trace.TraceInformation("progress-pre = " + progress.ToString() + " depth:" + depth + " => progress: " + progress + " + levelAdd:" + levelAddition + " * (famNo:" + familyNo + "  / famCount:" + parentFamilyList.Count + ") * (parentNo:" + parentNo + " / parentCount:" + parentXrefList.Count + ") = " + (double)(progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count)) + " " + parentXref.GetXrefName() + "=" + parent.GetName().ToString());
                    double progressPercent = progress + levelAddition * ((double)familyNo / parentFamilyList.Count) * ((double)parentNo / parentXrefList.Count);
                    if (latestPercent > progressPercent)
                    {
                      trace.TraceData(TraceEventType.Warning, 0, "Progress = backwards!!! " + progressPercent.ToString("P2") + "<" + latestPercent.ToString("P2"));
                    }
                    latestPercent = progressPercent;
                    trace.TraceInformation("Progress-post = " + progressPercent.ToString("P2"));
                    if (progressReporter != null)
                    {
                      progressReporter.ReportProgress(progressPercent * 100.0, progressDescription + ancestors.Count);
                      if (progressReporter.CheckIfStopRequested())
                      {
                        StopRequested = true;
                      }
                    }
                    if (!StopRequested)
                    {
                      LoadAncestors(familyTree, parent, ref ancestors, generations - 1, GetParentRelation(parent), stack2, progressPercent, progressReporter, progressDescription);
                    }
                  }
                  else
                  {
                    trace.TraceEvent(TraceEventType.Error, 0, "Error person " + parentXref.GetXrefName() + " not found in database!");
                  }
                  parentNo++;
                }
              }
              else
              {
                //trace.TraceEvent(TraceEventType.Information, 0, "family.GetParentList() " + familyXref.ToString() + " is null!");
              }
            }
            else
            {
              trace.TraceEvent(TraceEventType.Error, 0, "Error family " + familyXref.ToString() + " not found in database!");
            }
            familyNo++;
          }
        }
      }
    }
  }
}

