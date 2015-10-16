CREATE TABLE [dbo].[SecurityObjectPermission] (
    [SecurityObjectPermission] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PermissionId]             INT              NOT NULL,
    [SecurityObjectId]         UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_SecurityObjectPermission] PRIMARY KEY CLUSTERED ([SecurityObjectPermission] ASC),
    CONSTRAINT [FK_SecurityObjectPermission_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permission] ([PermissionId]),
    CONSTRAINT [FK_SecurityObjectPermission_SecurityObject] FOREIGN KEY ([SecurityObjectId]) REFERENCES [dbo].[SecurityObject] ([SecurityObjectId])
);

