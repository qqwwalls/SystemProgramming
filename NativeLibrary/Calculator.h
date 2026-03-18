#pragma once

#if defined(_WIN32)
#if defined(NATIVELIBRARY_EXPORTS)
#define NATIVELIBRARY_API __declspec(dllexport)
#else
#define NATIVELIBRARY_API __declspec(dllimport)
#endif
#else
#define NATIVELIBRARY_API __attribute__((visibility("default")))
#endif

class NATIVELIBRARY_API Calculator
{
public:
    Calculator();
    int Add(int left, int right);
    ~Calculator();
};

extern "C" NATIVELIBRARY_API Calculator* CreateCalculatorObject();
extern "C" NATIVELIBRARY_API void DeleteCalculatorObject(Calculator* obj);
extern "C" NATIVELIBRARY_API int Add(Calculator* obj, int a, int b);
