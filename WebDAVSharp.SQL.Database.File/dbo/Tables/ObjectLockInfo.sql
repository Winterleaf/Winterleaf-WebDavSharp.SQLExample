CREATE TABLE [dbo].[ObjectLockInfo] (
    [ObjectGuid]           UNIQUEIDENTIFIER NOT NULL,
    [Token]                UNIQUEIDENTIFIER CONSTRAINT [DF__ObjectLoc__Token__5B78929E] DEFAULT (newsequentialid()) NOT NULL,
    [isFolder]             BIT              NOT NULL,
    [LockScope]            INT              NOT NULL,
    [LockType]             INT              NOT NULL,
    [Owner]                UNIQUEIDENTIFIER NOT NULL,
    [RequestedLockTimeout] FLOAT (53)       NULL,
    [ExpirationDate]       DATETIME         NULL,
    [Depth]                INT              NOT NULL,
    [Path]                 NVARCHAR (MAX)   NOT NULL,
    [CreateDt]             DATETIME         NOT NULL,
    CONSTRAINT [PK_FileLockInfo] PRIMARY KEY CLUSTERED ([ObjectGuid] ASC, [Token] ASC),
    CONSTRAINT [FK_ObjectLockInfo_SecurityObject] FOREIGN KEY ([Owner]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);

