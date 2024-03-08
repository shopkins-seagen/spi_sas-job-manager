using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class Repository
    {
        private SasContext c;
        public Repository(SasContext context)
        {
            c = context;
        }

        public List<Job> GetJobs()
        {
            var jobs = new List<Job>();

                foreach (var j in c.Jobs)
                    jobs.Add(j);
         
            return jobs;
        }
        public DbResponse SaveNewJob(Job job)
        {
            var response = new DbResponse
            {
                IsOk = true
            };

            try
            {
                c.Jobs.Add(job);
                c.SaveChanges();
                response.Id = job.Id;
                response.Msg = $"Job '{job.Description}' added to the DB. Complete the configuration details";
            }
            catch (Exception ex)
            {
                response.IsOk = false;
                response.Msg = ex.Message;
            }


            return response;
        }

        public DbResponse UpdateJob(Job job)
        {
            var response = new DbResponse
            {
                IsOk = true
            };
            try
            {
                var entity = c.Jobs.Find(job.Id);
                if (entity == null)
                {
                    response.IsOk = false;
                    response.Msg = $"Unable to locate job id {job.Id} in database. Contact SPI";
                    return response;
                }
                c.Entry(entity).CurrentValues.SetValues(job);
                c.SaveChanges();
                response.Msg = $"Job '{job.Description}' updated in the database";

            }
            catch (Exception ex)
            {
                response.IsOk = false;
                response.Msg = ex.Message;
            }

            return response;
        }

        public Job? GetJob(int id)
        {
            return c.Jobs.Include("JobRuns").FirstOrDefault(x => x.Id == id);
        }

        public DbResponse RecordRunDetails(int id, List<string> msgs, DateTime started, DateTime finished, string status)
        {
            var response = new DbResponse()
            {
                IsOk = true
            };
            try
            {
                var entity = GetJob(id);
                if (entity == null)
                {
                    // make a logger and log it
                    response.Msg = $"The job ID={id} was not found in the database";
                    response.IsOk = false;
                    return response;
                }
                var run = new JobRun()
                {
                    JobId = id,
                    Started = started,
                    Completed = finished,
                    WorstFinding = status,
                    AppPoolIdentity=Environment.UserName
                };
                c.JobRuns.Add(run);
                foreach (var m in msgs)
                {
                    run.Messages.Add(new JobRunMsg()
                    {
                        Message = m
                    });
                }
                c.SaveChanges();
                response.Msg = $"Run details for Job Id={id} added to the database";

            }
            catch (Exception ex)
            {
                response.Msg = ex.Message;
                response.IsOk = false;
            }

            return response;
        }

        public void RecordSchedulerDetails(SchedulerRun run)
        {
            c.SchedulerRun.Add(run);
            c.SaveChanges();
        }

        public IEnumerable<JobRunMsg> GetJobRunMsgs(int id)
        {
            return c.JobRunMsgs.Where(x => x.JobRunId == id).ToList();
        }

        public List<JobRun> GetJobRuns(int id)
        {
            return  c.JobRuns.Where(x => x.JobId == id).ToList();
        }

        public DbResponse DeleteJob(int id)
        {
            var response = new DbResponse()
            {
                IsOk = true
            };
            try
            {
                var entity = c.Jobs.FirstOrDefault(x => x.Id == id);
                var desc = entity.Description;
                if (entity != null)
                {
                    c.Jobs.Remove(entity);
                    c.SaveChanges();
                }
                response.Msg = $"Job '{desc}' removed from the database";
            }
            catch (Exception ex)
            {
                response.IsOk = false;
                response.Msg = ex.Message;
            }

            return response;
        }
    }
}
