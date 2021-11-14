using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Controllers
{
    [Authorize]
    [Route("api/buckets")]
    [ApiController]
    public class BucketController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public BucketController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public JsonResult GetBuckets()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Buckets";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count == 0)
            {
                result.StatusCode = 404;
                result.Value = "No Buckets created!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }


        [HttpPost]
        public JsonResult PostBucket(Bucket bucket)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Buckets (Title) VALUES (@Title)";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Title", bucket.Title);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Bucket. It's on us...";
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = "Bucket Inserted Successfully!";
            return result;
        }

        [HttpPut]
        public JsonResult UpdateBucketTitle(Bucket bucketToUpdate)
        {
            JsonResult result = new JsonResult("");

            string query = @"UPDATE dbo.Buckets SET Title = @newTitle WHERE BucketID = @BucketId";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@BucketId", bucketToUpdate.BucketId);
                    myCommand.Parameters.AddWithValue("@newTitle", bucketToUpdate.Title);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 404;
                        result.Value = "No Bucket with this Id exists.";
                        return result;
                    }
                    myCon.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = "Bucket updated successfully!";
            return result;
        }

    }
}
