using Microsoft.EntityFrameworkCore;

namespace SuCaiFlow.Example;

public class SuCaiFlowDbContext(DbContextOptions<SuCaiFlowDbContext> dbContext) : DbContext(dbContext) {
}
