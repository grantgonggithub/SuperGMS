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
               var _item = context.Set<item>().Find(defaultPrimaryKey); // Use the primary key to query the primary record
                var query = context.Set<itemDetail>().Where<itemDetail>(x => x.ItemGID == _item.ItemGID).AsEnumerable();
                context.Set<itemDetail>().RemoveRange(query); // Delete all details based on foreign keys
                var _itemDetails = _item.itemDetail.Where(x => x.LineId % 2 == 0).ToList(); // Select records where lineid is even
                int i = 0;
                for (i=0;i< _itemDetails.Count(); i++)
                {
                    var d = new itemDetail {
                        GUID = Guid.NewGuid().ToString("N"),
                        ItemGID = _itemDetails[i].ItemGID,
                        LineId = i,
                        LineName= _itemDetails[i].LineName
                    };
                    context.Set<itemDetail>().Add(d); //Insert records whose lineid is even after changing their Numbers
                }

                var newOne = new itemDetail
                {
                    GUID = Guid.NewGuid().ToString("N"),
                    ItemGID = defaultPrimaryKey,
                    LineId = i+1,
                    LineName ="new_LineName"
                };
                context.Set<itemDetail>().Add(newOne); //Insert new detail

                context.SaveChanges(); // commit
            }
        }
    }
}
