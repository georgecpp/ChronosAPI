using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChronosAPI.Controllers
{
    [Authorize]
    [Route("api/Links")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public LinksController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public JsonResult GetLinks()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Links";
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
                result.Value = "Table is empty!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpPost]
        public JsonResult PostLink(Link link)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Links
           (TaskID,URL)
     VALUES
           (@TaskId, @URL)";
            string sqlDataSource = _appSettings.ChronosDBCon;

            DataTable Task = new DataTable();
            string selectQueryTask = @"SELECT * from dbo.Tasks";
            SqlDataReader taskReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                SqlCommand getAllTasks = new SqlCommand(selectQueryTask, myCon);
                taskReader = getAllTasks.ExecuteReader();
                Task.Load(taskReader);
                bool taskExists = Task.AsEnumerable().Any(row => link.TaskId == row.Field<int>("TaskID"));
                if (!taskExists)
                {
                    result.StatusCode = 404;
                    result.Value = "Task does not exist in Database!!!";
                    myCon.Close();
                    return result;
                }

                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@TaskId", link.TaskId);
                    myCommand.Parameters.AddWithValue("@URL", link.URL);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Link. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = link;
            return result;
        }

        [HttpPut]
        public JsonResult UpdateLink(Link link)
        {
            JsonResult result = new JsonResult("");

            string query = @"UPDATE dbo.Links SET URL = @URL WHERE LinkID=@LinkID";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@LinkID", link.LinkId);
                    myCommand.Parameters.AddWithValue("@URL", link.URL);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 404;
                        result.Value = "Failed to Update Link. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = link;
            return result;
        }

        [HttpDelete]
        public JsonResult DeleteLink(Link link)
        {
            string query = @" DELETE from dbo.Links where LinkID=@LinkID";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            DataTable linksTable = new DataTable();
            string selectQueryLinks = @"SELECT * from dbo.Links";
            SqlDataReader taskReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    myCon.Open();
                    SqlCommand getAllLinks = new SqlCommand(selectQueryLinks, myCon);
                    taskReader = getAllLinks.ExecuteReader();
                    linksTable.Load(taskReader);
                    bool taskExists = linksTable.AsEnumerable().Any(row => link.LinkId == row.Field<int>("LinkID"));
                    myCon.Close();
                    if (!taskExists)
                    {
                        result.StatusCode = 404;
                        result.Value = "Link does not exist in the table!";
                        myCon.Close();
                        return result;
                    }

                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@LinkID", link.LinkId);
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();
                    }
                }
            }
            result.StatusCode = 200;
            result.Value = "Delete successful!";
            return result;
        }
    }
}