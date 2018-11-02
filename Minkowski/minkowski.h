#include <iostream>
#include <boost/polygon/polygon.hpp>
  
extern  "C" {  	
	
	__declspec(dllexport) void  setData(int cntA, double* pntsA, int holesCnt, int* holesSizes, double* holesPoints, int cntB, double* pntsB);
	__declspec(dllexport) void  getSizes1(int* sizes);
	__declspec(dllexport) void  getSizes2(int* sizes1, int* sizes2);
	__declspec(dllexport) void  getResults(double* data, double* holesData);
	__declspec(dllexport) void  calculateNFP();
}
