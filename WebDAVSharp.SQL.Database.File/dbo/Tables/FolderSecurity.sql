CREATE TABLE [dbo].[FolderSecurity] (
    [pk_FolderSecurityId]  UNIQUEIDENTIFIER CONSTRAINT [DF_FolderSecurity_pk_FolderSecurityId] DEFAULT (newsequentialid()) NOT NULL,
    [fk_FolderId]          UNIQUEIDENTIFIER NOT NULL,
    [SecurityObjectId]     UNIQUEIDENTIFIER NOT NULL,
    [canListObjects]       BIT              NOT NULL,
    [canCreateFiles]       BIT              NOT NULL,
    [canCreateFolders]     BIT              NOT NULL,
    [canDelete]            BIT              NOT NULL,
    [canChangePermissions] BIT              NOT NULL,
    CONSTRAINT [PK_FolderSecurity] PRIMARY KEY CLUSTERED ([pk_FolderSecurityId] ASC),
    CONSTRAINT [FK_FolderSecurity_Folder] FOREIGN KEY ([fk_FolderId]) REFERENCES [dbo].[Folder] ([pk_FolderId]),
    CONSTRAINT [FK_FolderSecurity_SecurityObject] FOREIGN KEY ([SecurityObjectId]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [NonClusteredIndex-20150831-110533]
    ON [dbo].[FolderSecurity]([fk_FolderId] ASC, [SecurityObjectId] ASC);

