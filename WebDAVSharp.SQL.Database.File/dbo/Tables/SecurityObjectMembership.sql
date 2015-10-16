CREATE TABLE [dbo].[SecurityObjectMembership] (
    [Id]                       UNIQUEIDENTIFIER CONSTRAINT [DF_SecurityObjectMembership_Id] DEFAULT (newsequentialid()) NOT NULL,
    [SecurityObjectId]         UNIQUEIDENTIFIER NOT NULL,
    [MemberOfSecurityObjectId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_SecurityObjectMembership] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SecurityObjectMembership_SecurityObject] FOREIGN KEY ([SecurityObjectId]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId]),
    CONSTRAINT [FK_SecurityObjectMembership_SecurityObject1] FOREIGN KEY ([MemberOfSecurityObjectId]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);

