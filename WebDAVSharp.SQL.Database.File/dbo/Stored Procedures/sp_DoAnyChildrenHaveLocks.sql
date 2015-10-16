


create proc sp_DoAnyChildrenHaveLocks
	@ObjectGuid as uniqueidentifier = null
as
	
	create table #temp
	(
	
	objectguid uniqueidentifier ,
	isCollection bit

	)
declare @flag  bit
set @flag = 0

	if @ObjectGuid is null
	begin
	select @ObjectGuid = pk_folderid from folder where fk_ParentFolderId is null
	end

	insert into #temp select @ObjectGuid,1

	declare @count int, @lastCount int

	set @lastCount = 0
	set @count = 1

	while (@lastCount <> @count)
	begin
		set @lastCount = @count
		insert into #temp (objectguid,isCollection)
		select pk_folderid,convert(bit,1) from [folder] where 
		fk_ParentFolderId in (select objectguid from #temp) and pk_FolderId not in (select objectguid from #temp)
		select @count = count(*) from #temp
	end

	insert into #temp(objectguid,isCollection)
	select pk_FileId,convert(bit,0) from [file] where fk_FolderId in (select objectguid from #temp where isCollection =1)

	

	declare @c1 int, @c2 int
	set @c1=0;
	set @c2 =0
	select @c1  = count(*) from ObjectLockInfo a where a.objectguid in (select objectguid from #temp where isCollection = 1) and a.isFolder =1
	select @c2  = count(*) from ObjectLockInfo a where a.objectguid in (select objectguid from #temp where isCollection = 0) and a.isFolder =0
	drop table #temp

	if (@c1 + @c2>0)
	begin
	set @flag=1
	end

	select @flag [Exists]
