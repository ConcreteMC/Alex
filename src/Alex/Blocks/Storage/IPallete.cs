using Alex.API.Blocks.State;

namespace Alex.Blocks.Storage
{
    public interface IPallete<Tk>
    {
        uint GetId(Tk state);

        uint Add(Tk state);

        Tk Get(uint id);

        void Put(Tk objectIn, uint intKey);
    }
}