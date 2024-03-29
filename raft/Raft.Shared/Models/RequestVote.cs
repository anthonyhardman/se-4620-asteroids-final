namespace Raft.Shared.Models;

public class RequestVoteRequest
{
    public int Term { get; set; }
    public string CandidateId { get; set; }
    public int LastLogIndex { get; set; }
    public int LastLogTerm { get; set; }
}

public class RequestVoteResponse
{
    public int Term { get; set; }
    public bool VoteGranted { get; set; }
}
