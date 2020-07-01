using System;
using System.Collections.Generic;
using System.Text;

namespace BOG.SwissArmyKnife.Entity
{
    public enum ProcessState : int
    {
        Active = 1,
        Sunsetting = 2,
        CompletedSuccessfully = 3,
        Deadlocked = 4,
        MaxErrorsExceeded = 5,
        UnexpectedError = 6
    }

    public enum WorkflowStep : int
    {
        CreateQuestionBlock = 0,
        PushQuestionBlock = 1,
        PullQuestionBlock = 2,
        AnswerQuestionBlock = 3,
        PushAnswerBlock = 4,
        PullAnswerBlock = 5,
        MergeAnswers = 6
    }

    public enum ResearchState : int
    {
        Active = 1,
        Paused = 2,
        Sunsetting = 3,
        Finished = 4
    }

    public enum QuestionState : int
    {
        Pending = 0,
        Successful = 1,
        Failure = 2
    }

    public enum QuestionBlockState : int
    {
        Pending = 0,
        Processing = 1,
        Complete = 2
    }

    public enum OperatingMode : int
    {
        Standalone = 0,
        Master = 1,
        Worker = 2
    }
}
