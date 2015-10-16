CREATE TABLE [dbo].[File] (
    [pk_FileId]          UNIQUEIDENTIFIER CONSTRAINT [DF__File__pk_FileId__3864608B] DEFAULT (newsequentialid()) NOT NULL,
    [fk_FolderId]        UNIQUEIDENTIFIER NOT NULL,
    [Name]               VARCHAR (MAX)    NOT NULL,
    [isRevisioned]       BIT              NOT NULL,
    [CreateDt]           DATETIME         NOT NULL,
    [isDeleted]          BIT              NOT NULL,
    [MimeType]           VARCHAR (MAX)    NOT NULL,
    [Win32FileAttribute] INT              CONSTRAINT [DF_File_Win32FileAttribute] DEFAULT ((0)) NOT NULL,
    [Owner]              UNIQUEIDENTIFIER NOT NULL,
    [DeletedDt]          DATETIME         NULL,
    [DeletedBy]          UNIQUEIDENTIFIER NULL,
    [CreatedBy]          UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED ([pk_FileId] ASC),
    CONSTRAINT [FK_File_Folder] FOREIGN KEY ([fk_FolderId]) REFERENCES [dbo].[Folder] ([pk_FolderId]),
    CONSTRAINT [FK_File_SecurityObject] FOREIGN KEY ([Owner]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId]),
    CONSTRAINT [FK_File_SecurityObject1] FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId]),
    CONSTRAINT [FK_File_SecurityObject2] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_File_18_914102297__K2_K6_1_3_4_5_7_1912]
    ON [dbo].[File]([fk_FolderId] ASC, [isDeleted] ASC)
    INCLUDE([pk_FileId], [Name], [isRevisioned], [CreateDt], [MimeType]);

