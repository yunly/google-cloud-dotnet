// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Generated code. DO NOT EDIT!

namespace Google.Cloud.Asset.V1Beta1.Tests
{
    using Google.Api.Gax;
    using Google.Api.Gax.Grpc;
    using apis = Google.Cloud.Asset.V1Beta1;
    using Google.LongRunning;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Moq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>Generated unit tests</summary>
    public class GeneratedAssetServiceClientTest
    {
        [Fact]
        public void BatchGetAssetsHistory()
        {
            Mock<AssetService.AssetServiceClient> mockGrpcClient = new Mock<AssetService.AssetServiceClient>(MockBehavior.Strict);
            mockGrpcClient.Setup(x => x.CreateOperationsClient())
                .Returns(new Mock<Operations.OperationsClient>().Object);
            BatchGetAssetsHistoryRequest expectedRequest = new BatchGetAssetsHistoryRequest
            {
                Parent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString(),
            };
            BatchGetAssetsHistoryResponse expectedResponse = new BatchGetAssetsHistoryResponse();
            mockGrpcClient.Setup(x => x.BatchGetAssetsHistory(expectedRequest, It.IsAny<CallOptions>()))
                .Returns(expectedResponse);
            AssetServiceClient client = new AssetServiceClientImpl(mockGrpcClient.Object, null);
            string formattedParent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString();
            BatchGetAssetsHistoryResponse response = client.BatchGetAssetsHistory(formattedParent);
            Assert.Same(expectedResponse, response);
            mockGrpcClient.VerifyAll();
        }

        [Fact]
        public async Task BatchGetAssetsHistoryAsync()
        {
            Mock<AssetService.AssetServiceClient> mockGrpcClient = new Mock<AssetService.AssetServiceClient>(MockBehavior.Strict);
            mockGrpcClient.Setup(x => x.CreateOperationsClient())
                .Returns(new Mock<Operations.OperationsClient>().Object);
            BatchGetAssetsHistoryRequest expectedRequest = new BatchGetAssetsHistoryRequest
            {
                Parent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString(),
            };
            BatchGetAssetsHistoryResponse expectedResponse = new BatchGetAssetsHistoryResponse();
            mockGrpcClient.Setup(x => x.BatchGetAssetsHistoryAsync(expectedRequest, It.IsAny<CallOptions>()))
                .Returns(new Grpc.Core.AsyncUnaryCall<BatchGetAssetsHistoryResponse>(Task.FromResult(expectedResponse), null, null, null, null));
            AssetServiceClient client = new AssetServiceClientImpl(mockGrpcClient.Object, null);
            string formattedParent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString();
            BatchGetAssetsHistoryResponse response = await client.BatchGetAssetsHistoryAsync(formattedParent);
            Assert.Same(expectedResponse, response);
            mockGrpcClient.VerifyAll();
        }

        [Fact]
        public void BatchGetAssetsHistory2()
        {
            Mock<AssetService.AssetServiceClient> mockGrpcClient = new Mock<AssetService.AssetServiceClient>(MockBehavior.Strict);
            mockGrpcClient.Setup(x => x.CreateOperationsClient())
                .Returns(new Mock<Operations.OperationsClient>().Object);
            BatchGetAssetsHistoryRequest request = new BatchGetAssetsHistoryRequest
            {
                Parent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString(),
                AssetNames = { },
                ContentType = ContentType.Unspecified,
                ReadTimeWindow = new TimeWindow(),
            };
            BatchGetAssetsHistoryResponse expectedResponse = new BatchGetAssetsHistoryResponse();
            mockGrpcClient.Setup(x => x.BatchGetAssetsHistory(request, It.IsAny<CallOptions>()))
                .Returns(expectedResponse);
            AssetServiceClient client = new AssetServiceClientImpl(mockGrpcClient.Object, null);
            BatchGetAssetsHistoryResponse response = client.BatchGetAssetsHistory(request);
            Assert.Same(expectedResponse, response);
            mockGrpcClient.VerifyAll();
        }

        [Fact]
        public async Task BatchGetAssetsHistoryAsync2()
        {
            Mock<AssetService.AssetServiceClient> mockGrpcClient = new Mock<AssetService.AssetServiceClient>(MockBehavior.Strict);
            mockGrpcClient.Setup(x => x.CreateOperationsClient())
                .Returns(new Mock<Operations.OperationsClient>().Object);
            BatchGetAssetsHistoryRequest request = new BatchGetAssetsHistoryRequest
            {
                Parent = new Google.Api.Gax.ResourceNames.ProjectName("[PROJECT]").ToString(),
                AssetNames = { },
                ContentType = ContentType.Unspecified,
                ReadTimeWindow = new TimeWindow(),
            };
            BatchGetAssetsHistoryResponse expectedResponse = new BatchGetAssetsHistoryResponse();
            mockGrpcClient.Setup(x => x.BatchGetAssetsHistoryAsync(request, It.IsAny<CallOptions>()))
                .Returns(new Grpc.Core.AsyncUnaryCall<BatchGetAssetsHistoryResponse>(Task.FromResult(expectedResponse), null, null, null, null));
            AssetServiceClient client = new AssetServiceClientImpl(mockGrpcClient.Object, null);
            BatchGetAssetsHistoryResponse response = await client.BatchGetAssetsHistoryAsync(request);
            Assert.Same(expectedResponse, response);
            mockGrpcClient.VerifyAll();
        }

    }
}
