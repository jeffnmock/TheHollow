using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheHollow.PrepareThePit
{
    class Program
    {
        static readonly string[] s_toInject = {
          // alternatively "MyAssembly, PublicKey=0024000004800000... etc."
          "PitMod"
        };

        static void Main(string[] args)
        {
            const string THIRD_PARTY_ASSEMBLY_PATH = @"..\..\..\original\ThePit.exe";

            var parameters = new ReaderParameters();
            var asm = ModuleDefinition.ReadModule(THIRD_PARTY_ASSEMBLY_PATH, parameters);
            foreach (var toInject in s_toInject)
            {
                var ca = new CustomAttribute(
                  asm.Import(typeof(InternalsVisibleToAttribute).GetConstructor(new[] {
                      typeof(string)})));
                ca.ConstructorArguments.Add(new CustomAttributeArgument(asm.TypeSystem.String, toInject));
                asm.Assembly.CustomAttributes.Add(ca);
            }


            foreach (var item in asm.Types)
            {
                //if (item.IsNotPublic)
                //{
                //    item.IsPublic = true;
                //}
                //foreach (var method in item.Methods)
                //{
                //    if (!method.IsPublic)
                //        method.IsPublic = true;

                //    if (!method.IsVirtual && !method.IsStatic && !method.IsConstructor)
                //        method.IsVirtual = true;
                //}

                //foreach (var field in item.Fields)
                //{
                //    if (!field.IsPublic)
                //        field.IsPublic = true;
                //}

                ModifyType(item);
            }

            asm.Write(@"..\..\..\lib\ThePit.exe");
            // note if the assembly is strongly-signed you need to resign it like
            // asm.Write(@"c:\folder-modified\ThirdPartyAssembly.dll", new WriterParameters {
            //   StrongNameKeyPair = new StrongNameKeyPair(File.ReadAllBytes(@"c:\MyKey.snk"))
            // });
        }

        static void ModifyType(TypeDefinition item)
        {
            if(item.Name == "PsiPointsEffect")
            {
                int i = 0;
            }
            if (item.IsNotPublic)
            {
                item.IsPublic = true;
            }
            if (item.IsNestedPrivate)
            {
                item.IsNestedPublic = true;
            }
            foreach (var method in item.Methods)
            {
                if (!method.IsPublic)
                    method.IsPublic = true;

                if (!method.IsVirtual && !method.IsStatic && !method.IsConstructor)
                    method.IsVirtual = true;
            }

            foreach (var field in item.Fields)
            {
                if (!field.IsPublic)
                    field.IsPublic = true;
            }

            foreach(var nestedItem in item.NestedTypes)
            {
                ModifyType(nestedItem);
            }
        }
    }
}
