using AutoMapper;

namespace Employee.Management.System.Common.Api
{
    public class AutoMapperProfile<TSource, TDestination> : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TSource, TDestination>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember, context) =>
                    srcMember != null && !(srcMember.GetType().IsValueType && srcMember.Equals(Activator.CreateInstance(srcMember.GetType())))
                ));
        }
    }
}
