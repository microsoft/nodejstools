// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace TestUtilities.Nodejs
{
    public class NodejsTestData
    {
        private const string DataSourcePath = @"Nodejs\Tests\TestData";

        public static void Deploy(bool includeTestData = true)
        {
            TestData.Deploy(DataSourcePath, includeTestData);
        }
    }
}

