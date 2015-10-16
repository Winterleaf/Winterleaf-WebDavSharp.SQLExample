CREATE TABLE [dbo].[FileCatalogEntry] (
    [pk_FileCatalogEntryId] UNIQUEIDENTIFIER CONSTRAINT [DF_FileCatalog_pk_ContentId] DEFAULT (newsequentialid()) NOT NULL,
    [binaryData]            VARBINARY (MAX)  NOT NULL,
    CONSTRAINT [PK_FileCatalog] PRIMARY KEY CLUSTERED ([pk_FileCatalogEntryId] ASC)
);

