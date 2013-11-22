using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm{
    public class ReaderPackageJsonSource : IPackageJsonSource{
        public ReaderPackageJsonSource(TextReader reader){
            try{
                Package = JsonConvert.DeserializeObject(reader.ReadToEnd());
            } catch (JsonReaderException jre){
                WrapExceptionAndRethrow(jre);
            } catch (JsonSerializationException jse){
                WrapExceptionAndRethrow(jse);
            } catch (FormatException fe){
                WrapExceptionAndRethrow(fe);
            } catch (ArgumentException ae){
                throw new PackageJsonException(
                    string.Format(@"Error reading package.json. The file may be parseable JSON but may contain objects with duplicate properties.

The following error occurred:

{0}", ae.Message),
                    ae);
            }
        }

        private void WrapExceptionAndRethrow(
            Exception ex){
            throw new PackageJsonException(
                string.Format(@"Unable to read package.json. Please ensure the file is valid JSON.

Reading failed because the following error occurred:

{0}", ex.Message),
                ex);
        }

        public dynamic Package { get; private set; }
    }
}