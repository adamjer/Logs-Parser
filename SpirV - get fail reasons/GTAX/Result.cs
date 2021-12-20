using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class Result : IComparable
    {
        [JsonProperty(PropertyName = "gta_status")]
        public String GTAStatus { get; set; }

        [JsonProperty(PropertyName = "client_rebooted")]
        public String ClientRebooted { get; set; }

        [JsonProperty(PropertyName = "id")]
        public String ID { get; set; }

        [JsonProperty(PropertyName = "deadline")]
        public String Deadline { get; set; }

        [JsonProperty(PropertyName = "expected_subtasks_count")]
        public String ExpectedSubtasksCount { get; set; }

        [JsonProperty(PropertyName = "gta_test_identifier")]
        public String GtaTestIdentifier { get; set; }

        [JsonProperty(PropertyName = "failed_publish_attempts")]
        public String FailedPublishAttempts { get; set; }

        [JsonProperty(PropertyName = "status")]
        public String Status { get; set; }

        [JsonProperty(PropertyName = "logs_uri")]
        public String LogsUri { get; set; }

        [JsonProperty(PropertyName = "test_identifier_id")]
        public String TestIdentifierID { get; set; }

        [JsonProperty(PropertyName = "started_date")]
        public String StartedDate { get; set; }

        [JsonProperty(PropertyName = "publishing_dumps")]
        public String PublishingDumps { get; set; }

        [JsonProperty(PropertyName = "processing_status")]
        public String ProcessingStatus { get; set; }

        [JsonProperty(PropertyName = "dumps_uri")]
        public String DumpsUri { get; set; }

        [JsonProperty(PropertyName = "plugin_started_date")]
        public String PluginStartedDate { get; set; }

        [JsonProperty(PropertyName = "publishing_logs")]
        public String PublishingLogs { get; set; }

        [JsonProperty(PropertyName = "analyze_status")]
        public String AnalyzeStatus { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public Artifacts Artifacts { get; set; }

        [JsonProperty(PropertyName = "plugin_completed_date")]
        public String PluginCompletedDate { get; set; }

        [JsonProperty(PropertyName = "jobset_task_id")]
        public String JobsetTaskID { get; set; }

        [JsonProperty(PropertyName = "submission_type")]
        public String SubmissionType { get; set; }

        /*[JsonProperty(PropertyName = "runner_artifacts")]
        public IList<String> RunnerArtifacts { get; set; }*/

        [JsonProperty(PropertyName = "sync_artifacts_date")]
        public String SyncArtifactsDate { get; set; }

        [JsonProperty(PropertyName = "aborting_task_result_id")]
        public String AbortingTaskEesultID { get; set; }

        [JsonProperty(PropertyName = "gta_result_key")]
        public String GtaResultKey { get; set; }

        [JsonProperty(PropertyName = "gdhm_uri")]
        public String GDHMUri { get; set; }

        [JsonProperty(PropertyName = "post_processing_date")]
        public String PostProcessingDate { get; set; }
        
        [JsonProperty(PropertyName = "primary_reason_id")]
        public String PrimaryReasonID { get; set; }

        [JsonProperty(PropertyName = "gta_result_type")]
        public String GtaResultType { get; set; }

        [JsonProperty(PropertyName = "performance_uri")]
        public String PerformanceUri { get; set; }

        [JsonProperty(PropertyName = "published_date")]
        public String PublishedDate { get; set; }

        [JsonProperty(PropertyName = "primary_reason_execution_mode")]
        public String PrimaryReasonExecutionMode { get; set; }

        [JsonProperty(PropertyName = "job_id")]
        public String JobID { get; set; }

        [JsonProperty(PropertyName = "completed_date")]
        public String CompletedDate { get; set; }

        [JsonProperty(PropertyName = "primary_reason")]
        public String PrimaryReason { get; set; }

        [JsonProperty(PropertyName = "command")]
        public String Command { get; set; }

        [JsonProperty(PropertyName = "job_name")]
        public String JobName { get; set; }

        [JsonProperty(PropertyName = "task_order_id")]
        public String TaskOrderID { get; set; }

        [JsonProperty(PropertyName = "phase_name")]
        public String PhaseName { get; set; }

        [JsonProperty(PropertyName = "phase_id")]
        public String PhaseID { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public String ClientID { get; set; }

        [JsonProperty(PropertyName = "client_name")]
        public String ClientName { get; set; }

        [JsonProperty(PropertyName = "business_attributes")]
        public BusinessAttributes BusinessAttributes { get; set; }

        [JsonProperty(PropertyName = "test_identifier")]
        public String TestIdentifier { get; set; }

        public int CompareTo(object obj)
        {
            Result other = (Result)obj;
            if (other.BusinessAttributes.ItemName == null)
                return 0;
            if (this.BusinessAttributes.ItemName == null)
                return 1;

            return this.BusinessAttributes.ItemName.CompareTo(other.BusinessAttributes.ItemName);
        }
    }
}
