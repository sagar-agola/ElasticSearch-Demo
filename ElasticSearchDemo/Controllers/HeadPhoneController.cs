using ElasticSearchDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticSearchDemo.Controllers;

[Route("api/headphones")]
[ApiController]
public class HeadPhoneController : ControllerBase
{
    private const string _indexName = "headphones";
    private const string _analyzerName = "custom-analyzer";
    private readonly IElasticClient _elasticClient;

    public HeadPhoneController(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    [HttpGet("create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        try
        {
            ExistsResponse existsResponse = await _elasticClient.Indices.ExistsAsync(_indexName);

            if (existsResponse.Exists == false)
            {
                CreateIndexResponse response = await _elasticClient.Indices.CreateAsync(_indexName, i => i
                    .Settings(s => s
                        .Analysis(a => a
                            .Analyzers(an => an
                                .Custom(_analyzerName, x => x
                                    .Tokenizer("keyword")
                                    .Filters("lowercase"))
                            )
                        )
                    )
                    .Map<HeadPhoneItem>(mp => mp
                        .Properties(ps => ps
                            .IntegerRange(n => n.Name(h => h.Id))
                            .Text(t => t
                                .Name(n => n.Title)
                                .Analyzer(_analyzerName)
                            )
                            .Text(t => t
                                .Name(n => n.Color)
                                .Analyzer(_analyzerName)
                            )
                            .Text(t => t
                                .Name(n => n.Type)
                                .Analyzer(_analyzerName)
                            )
                        )
                    )
                );

                return Ok(response.IsValid);
            }

            return BadRequest("Index already exists");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("filter")]
    public async Task<IActionResult> GetHeadPhones([FromQuery] int start, [FromQuery] int limit, HeadPhoneItem model)
    {
        try
        {
            //QuerySqlResponse response = await _elasticClient.Sql.QueryAsync(q => q.Query("SELECT * FROM headphones"));

            //foreach (SqlColumn column in response.Columns)
            //{

            //}

            //foreach (SqlRow row in response.Rows)
            //{
            //    headPhones.Add(new HeadPhoneItem
            //    {
            //        Title = row[0].As<string>()
            //    });
            //}

            ISearchResponse<HeadPhoneItem> response = await _elasticClient.SearchAsync<HeadPhoneItem>(x => x
                .Index(_indexName)
                .From(start)
                .Size(limit)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Wildcard(t => t.Title, model.Title),
                            m => m.Wildcard(t => t.Color, model.Color)
                        )
                    )
                )
            );

            if (response.IsValid)
            {
                List<HeadPhoneItem> headPhones = response.Hits.Select(x => new HeadPhoneItem
                {
                    Id = x.Id,
                    Title = x.Source.Title,
                    Color = x.Source.Color,
                    Type = x.Source.Type,
                    MRP = x.Source.MRP,
                    Price = x.Source.Price,
                    Rating = x.Source.Rating,
                    TotalRatings = x.Source.TotalRatings
                }).ToList();

                return Ok(headPhones);
            }

            return BadRequest("Error occured");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        try
        {
            CountResponse response = await _elasticClient.CountAsync<HeadPhoneItem>(x => x.Index(_indexName).Query(x => x.MatchAll()));

            if (response.IsValid)
            {
                return Ok(response.Count);
            }

            return BadRequest("Error occured");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(HeadPhoneItem model)
    {
        try
        {
            IndexResponse response = await _elasticClient.IndexAsync(model, x => x.Index(_indexName));

            if (response.IsValid)
            {
                return Ok(response.Id);
            }

            return BadRequest("Error occured");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, HeadPhoneItem model)
    {
        try
        {
            UpdateResponse<HeadPhoneItem> response = await _elasticClient.UpdateAsync(
                new DocumentPath<HeadPhoneItem>(
                    new Id(
                        id
                    )
                ),
                x => x.Index(_indexName).Doc(model)
            );

            if (response.IsValid)
            {
                return Ok(response.Id);
            }

            return BadRequest("Error occured");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            DeleteResponse response = await _elasticClient.DeleteAsync(
                new DocumentPath<HeadPhoneItem>(
                    new Id(
                        id
                    )
                )
            );

            if (response.IsValid)
            {
                return Ok(response.Id);
            }

            return BadRequest("Error occured");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("bulk-index")]
    public async Task<IActionResult> BulkIndex()
    {
        try
        {
            List<HeadPhoneItem> headPhones = await HeadPhoneItem.GetData();

            CountdownEvent waitHandler = new(1);

            BulkAllObservable<HeadPhoneItem> response = _elasticClient.BulkAll(headPhones, x => x.Index(_indexName));

            response.Subscribe(new BulkAllObserver(onCompleted: () => waitHandler.Signal()));
            waitHandler.Wait();

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
