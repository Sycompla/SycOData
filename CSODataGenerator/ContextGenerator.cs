﻿using Ac4yClassModule.Class;
using Ac4yClassModule.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSODataGenerator
{
    class ContextGenerator
    {

        #region members

        public string OutputPath { get; set; }
        public string Namespace { get; set; }
        public string References { get; set; }
        public CSODataGeneratorParameter Parameter { get; set; }

        public Type Type { get; set; }

        private const string TemplateExtension = ".csT";
        
        private const string ClassCodeMask = "#classCode#";
        private const string NamespaceMask = "#namespace#";
        private const string PlanObjectReferenceMask = "#planObjectReference#";
        private const string DbSetsMask = "#dbSets#";
        private const string EntitiesMask = "#entities#";
        private const string EntityMask = "#entity#";
        private const string EntityOneMask = "#entityOne#";
        private const string PropertyManyMask = "#propertyMany#";
        private const string ReferencePropertyMask = "#referenceProperty#";
        private const string ConnectionsMask = "#connections#";

        private const string DbSetsText = "public DbSet<#classCode#> #classCode#s { get; set; }";
        private const string EntitiesText = "modelBuilder.Entity<#classCode#>().ToTable(\"#classCode#s\");";

        private const string ClassCodeAsVariableMask = "#classCodeAsVariable#";

        #endregion members

        public string ReadIntoString(string fileName)
        {

            string textFile = "Templates/EFContextTPC4CORE3/" + fileName + TemplateExtension;

            return File.ReadAllText(textFile);

        } // ReadIntoString

        public void WriteOut(string text, string fileName, string outputPath)
        {
            File.WriteAllText(outputPath + fileName + ".cs", text);

        }

        public string GetNameWithLowerFirstLetter(String Code)
        {
            return
                char.ToLower(Code[0])
                + Code.Substring(1)
                ;

        } // GetNameWithLowerFirstLetter

        public string GetHead()
        {

            return ReadIntoString("Head")
                        .Replace(PlanObjectReferenceMask, Type.Namespace)
                        .Replace(NamespaceMask, Namespace + "Cap")
                        ;

        }

        public string GetFoot()
        {
            return
                ReadIntoString("Foot")
                        ;

        }

        public string GetMethods()
        {
            string entitiesText = "";
            string dbSetsText = "";
            List<Ac4yClass> connectionClasses = new List<Ac4yClass>();
            string editedConnectionsText = "";

            foreach(PlanObjectReference planObject in Parameter.PlanObjectReferenceList)
            {
                Ac4yClass ac4yClass = new Ac4yClassHandler().GetAc4yClassFromType(planObject.classType);

                foreach(Ac4yProperty property in ac4yClass.PropertyList)
                {
                    if(property.NavigationProperty == true)
                    {
                        connectionClasses.Add(ac4yClass);
                        break;
                    }
                }
            }

            foreach(Ac4yClass ac4yClass in connectionClasses)
            {
                string connectionsText = ReadIntoString("Connections")
                                            .Replace(EntityMask, ac4yClass.Name)
                                            .Replace(PropertyManyMask, ac4yClass.Name.Replace("Ac4y", "") + "List")
                                            ;

                foreach (Ac4yProperty property in ac4yClass.PropertyList)
                {

                    if (property.NavigationProperty == true)
                    {
                        connectionsText = connectionsText.Replace(EntityOneMask, property.Name)
                                                            .Replace(ReferencePropertyMask, property.Name + "Id");
                    }
                }

                editedConnectionsText = editedConnectionsText + connectionsText;
            }

            foreach(PlanObjectReference planObject in Parameter.PlanObjectReferenceList)
            {
                entitiesText = entitiesText + EntitiesText.Replace(ClassCodeMask, planObject.className) + "\n";
            }

            foreach (PlanObjectReference planObject in Parameter.PlanObjectReferenceList)
            {
                dbSetsText = dbSetsText + DbSetsText.Replace(ClassCodeMask, planObject.className) + "\n";
            }

            return
                ReadIntoString("Methods")
                        .Replace(EntitiesMask, entitiesText)
                        .Replace(DbSetsMask, dbSetsText)
                        .Replace(ConnectionsMask, editedConnectionsText)
                ;
        }

        public ContextGenerator Generate()
        {

            string result = null;

            result += GetHead();

            result += GetMethods();

            result += GetFoot();

            WriteOut(result, "Context", OutputPath);

            return this;

        } // Generate

        public ContextGenerator Generate(Type type)
        {

            Type = type;

            return Generate();

        } // Generate

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

    }

} // EFGenerala