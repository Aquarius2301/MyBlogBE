using System;
using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implementations;

public class PictureRepository : Repository<Picture>, IPictureRepository
{
    public PictureRepository(MyBlogContext context)
        : base(context) { }
}
