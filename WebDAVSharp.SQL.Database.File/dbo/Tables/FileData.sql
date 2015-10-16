CREATE TABLE [dbo].[FileData] (
    [pk_FileDataId] UNIQUEIDENTIFIER CONSTRAINT [DF__FileData__pk_Fil__3B40CD36] DEFAULT (newsequentialid()) NOT NULL,
    [fk_FileId]     UNIQUEIDENTIFIER NOT NULL,
    [Revision]      INT              NOT NULL,
    [Size]          INT              NOT NULL,
    [CreateDt]      DATETIME         NOT NULL,
    [fk_CatalogId]  UNIQUEIDENTIFIER NOT NULL,
    [fk_ContentId]  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_FileData] PRIMARY KEY CLUSTERED ([pk_FileDataId] ASC),
    CONSTRAINT [FK_FileData_Catalog] FOREIGN KEY ([fk_CatalogId]) REFERENCES [dbo].[Catalog] ([pk_CatalogId]),
    CONSTRAINT [FK_FileData_File] FOREIGN KEY ([fk_FileId]) REFERENCES [dbo].[File] ([pk_FileId])
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_FileData_18_962102468__K2_K6_K3_1_4_5_7_1912]
    ON [dbo].[FileData]([fk_FileId] ASC, [fk_CatalogId] ASC, [Revision] ASC)
    INCLUDE([pk_FileDataId], [Size], [CreateDt], [fk_ContentId]);

