// Generated by using Rcpp::compileAttributes() -> do not edit by hand
// Generator token: 10BE3573-1514-4C36-9D1C-5A225CD40393

#include <Rcpp.h>

using namespace Rcpp;

// read_mzpack
List read_mzpack(CharacterVector filepath);
RcppExport SEXP _mzkit_read_mzpack(SEXP filepathSEXP) {
BEGIN_RCPP
    Rcpp::RObject rcpp_result_gen;
    Rcpp::RNGScope rcpp_rngScope_gen;
    Rcpp::traits::input_parameter< CharacterVector >::type filepath(filepathSEXP);
    rcpp_result_gen = Rcpp::wrap(read_mzpack(filepath));
    return rcpp_result_gen;
END_RCPP
}

static const R_CallMethodDef CallEntries[] = {
    {"_mzkit_read_mzpack", (DL_FUNC) &_mzkit_read_mzpack, 1},
    {NULL, NULL, 0}
};

RcppExport void R_init_mzkit(DllInfo *dll) {
    R_registerRoutines(dll, NULL, CallEntries, NULL, NULL);
    R_useDynamicSymbols(dll, FALSE);
}