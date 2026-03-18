cmake_minimum_required(VERSION 3.16)
project(CalculatorLibrary LANGUAGES CXX)

add_library(calculator SHARED
    Calculator.cpp
    CalculatorApi.cpp
)

target_compile_features(calculator PRIVATE cxx_std_17)
target_include_directories(calculator PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})

set_target_properties(calculator PROPERTIES
    OUTPUT_NAME calculator
    POSITION_INDEPENDENT_CODE ON
)
