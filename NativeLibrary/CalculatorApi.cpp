#include "Calculator.h"

extern "C" NATIVELIBRARY_API Calculator* CreateCalculatorObject()
{
    return new Calculator();
}

extern "C" NATIVELIBRARY_API void DeleteCalculatorObject(Calculator* obj)
{
    delete obj;
}

extern "C" NATIVELIBRARY_API int Add(Calculator* obj, int a, int b)
{
    if (obj == nullptr)
    {
        return 0;
    }

    return obj->Add(a, b);
}
