CREATE TABLE [dbo].[FileSecurity] (
    [pk_FileSecurityId] UNIQUEIDENTIFIER CONSTRAINT [DF_FileSecurity_pk_FileSecurityId] DEFAULT (newsequentialid()) NOT NULL,
    [fk_FileId]         UNIQUEIDENTIFIER NOT NULL,
    [SecurityObjectId]  UNIQUEIDENTIFIER NOT NULL,
    [canRead]           BIT              NOT NULL,
    [canWrite]          BIT              NOT NULL,
    [CanDelete]         BIT              NOT NULL,
    CONSTRAINT [PK_FileSecurity] PRIMARY KEY CLUSTERED ([pk_FileSecurityId] ASC),
    CONSTRAINT [FK_FileSecurity_File] FOREIGN KEY ([fk_FileId]) REFERENCES [dbo].[File] ([pk_FileId]),
    CONSTRAINT [FK_FileSecurity_SecurityObject] FOREIGN KEY ([SecurityObjectId]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [NonClusteredIndex-20150831-111732]
    ON [dbo].[FileSecurity]([fk_FileId] ASC, [SecurityObjectId] ASC);

