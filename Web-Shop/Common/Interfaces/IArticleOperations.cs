﻿using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IArticleOperations : IService
    {
        [OperationContract]
        Task<List<Article>> GetArticles(string category);
        [OperationContract]
        Task<Tuple<bool, string>> CheckArticles(List<ChartItem> articles);
        [OperationContract]
        Task<string> PutInChart(List<ChartItem> articles);
        [OperationContract]
        Task GetPerviousState();
    }
}
