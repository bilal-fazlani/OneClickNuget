using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace OneClickNuget.WPF
{
    class StateManager
    {
        static ModelState _modelState;

        const string FileName = "modelstate";

        static StateManager()
        {
            bool fileExists = File.Exists(FileName);

            if (fileExists)
            {
                using (var fileStream = new FileStream(FileName, FileMode.Open))
                {
                    var binaryFormatter = new BinaryFormatter();
                    try
                    {
                        _modelState = (ModelState) binaryFormatter.Deserialize(fileStream);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            File.Delete(FileName);
                        }
                        catch (Exception) { }
                        finally
                        {
                            _modelState = new ModelState();
                        }
                    }
                }
            }
            else
            {
                _modelState = new ModelState();
            }
        }

        public static ModelState Get()
        {
            return _modelState;
        }

        public static void Save(ModelState modelState)
        {
            _modelState = modelState;

            using (var fileStream = new FileStream(FileName, FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, _modelState);
            }
        }
    }
}
