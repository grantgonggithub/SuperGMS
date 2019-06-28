using System;
using UnitTest.EFModel.Models;
using TestModel.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSameKeyException
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new demoContext())
            {
                var defaultPrimaryKey = "TestPK1";
                var _item = context.Set<item>().Find(defaultPrimaryKey); // query all details
              
                context.Set<item>().Remove(_item); // Delete all details based on foreign keys
            
                var d = new item {
                        ItemGID = _item.ItemGID,
                    };
                    context.Set<item>().Add(d);
               
                context.SaveChanges(); // commit
            }
        }
    }
}
