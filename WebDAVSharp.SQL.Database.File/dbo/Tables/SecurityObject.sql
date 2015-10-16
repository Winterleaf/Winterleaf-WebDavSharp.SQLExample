CREATE TABLE [dbo].[SecurityObject] (
    [SecurityObjectId]  UNIQUEIDENTIFIER CONSTRAINT [DF__SecurityO__Secur__1209AD79] DEFAULT (newsequentialid()) NOT NULL,
    [ActiveDirectoryId] UNIQUEIDENTIFIER NULL,
    [FullName]          VARCHAR (MAX)    NOT NULL,
    [Username]          VARCHAR (MAX)    NOT NULL,
    [EmailAddress]      VARCHAR (MAX)    NOT NULL,
    [IsGroup]           BIT              CONSTRAINT [DF_SecurityObject_IsGroup] DEFAULT ((0)) NOT NULL,
    [LastLogInOn]       DATETIME         CONSTRAINT [DF_SecurityObject_LastLogInOn] DEFAULT (getdate()) NULL,
    [IsActive]          BIT              CONSTRAINT [DF_SecurityObject_IsActive] DEFAULT ((1)) NOT NULL,
    [HomeFolder]        UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([SecurityObjectId] ASC),
    CONSTRAINT [FK_SecurityObject_Folder] FOREIGN KEY ([HomeFolder]) REFERENCES [dbo].[Folder] ([pk_FolderId])
);

