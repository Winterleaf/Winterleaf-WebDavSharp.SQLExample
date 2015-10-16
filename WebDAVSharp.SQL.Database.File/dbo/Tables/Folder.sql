CREATE TABLE [dbo].[Folder] (
    [pk_FolderId]            UNIQUEIDENTIFIER CONSTRAINT [DF__Folder__pk_Folde__3E1D39E1] DEFAULT (newsequentialid()) NOT NULL,
    [fk_ParentFolderId]      UNIQUEIDENTIFIER NULL,
    [Name]                   VARCHAR (MAX)    NOT NULL,
    [CreateDt]               DATETIME         CONSTRAINT [DF_Folder_CreateDt] DEFAULT (getdate()) NOT NULL,
    [fk_CatalogCollectionId] UNIQUEIDENTIFIER NOT NULL,
    [Win32FileAttribute]     INT              CONSTRAINT [DF_Folder_Win32FileAttribute] DEFAULT ((0)) NOT NULL,
    [isDeleted]              BIT              CONSTRAINT [DF_Folder_isDeleted] DEFAULT ((0)) NOT NULL,
    [DeletedDt]              DATETIME         NULL,
    [DeletedBy]              UNIQUEIDENTIFIER NULL,
    [Owner]                  UNIQUEIDENTIFIER NOT NULL,
    [CreatedBy]              UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Folder] PRIMARY KEY CLUSTERED ([pk_FolderId] ASC),
    CONSTRAINT [FK_Folder_CatalogCollection] FOREIGN KEY ([fk_CatalogCollectionId]) REFERENCES [dbo].[CatalogCollection] ([pk_CatalogCollectionId]),
    CONSTRAINT [FK_Folder_Folder] FOREIGN KEY ([fk_ParentFolderId]) REFERENCES [dbo].[Folder] ([pk_FolderId]),
    CONSTRAINT [FK_Folder_SecurityObject] FOREIGN KEY ([Owner]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId]),
    CONSTRAINT [FK_Folder_SecurityObject1] FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId]),
    CONSTRAINT [FK_Folder_SecurityObject2] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);

