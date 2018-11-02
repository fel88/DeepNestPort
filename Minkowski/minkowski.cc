#include "minkowski.h"


#include <fstream>
#include <iostream>
#include <boost/polygon/polygon.hpp>
#include <string>
#include <iostream>
#include <sstream>
#include <limits>

#undef min
#undef max

typedef boost::polygon::point_data<int> point;
typedef boost::polygon::polygon_set_data<int> polygon_set;
typedef boost::polygon::polygon_with_holes_data<int> polygon;
typedef std::pair<point, point> edge;
using namespace boost::polygon::operators;
using namespace std;;

void convolve_two_segments(std::vector<point>& figure, const edge& a, const edge& b) {
  using namespace boost::polygon;
  figure.clear();
  figure.push_back(point(a.first));
  figure.push_back(point(a.first));
  figure.push_back(point(a.second));
  figure.push_back(point(a.second));
  convolve(figure[0], b.second);
  convolve(figure[1], b.first);
  convolve(figure[2], b.first);
  convolve(figure[3], b.second);
}

template <typename itrT1, typename itrT2>
void convolve_two_point_sequences(polygon_set& result, itrT1 ab, itrT1 ae, itrT2 bb, itrT2 be) {
  using namespace boost::polygon;
  if(ab == ae || bb == be)
    return;
  point first_a = *ab;
  point prev_a = *ab;
  std::vector<point> vec;
  polygon poly;
  ++ab;
  for( ; ab != ae; ++ab) {
    point first_b = *bb;
    point prev_b = *bb;
    itrT2 tmpb = bb;
    ++tmpb;
    for( ; tmpb != be; ++tmpb) {
      convolve_two_segments(vec, std::make_pair(prev_b, *tmpb), std::make_pair(prev_a, *ab));
      set_points(poly, vec.begin(), vec.end());
      result.insert(poly);
      prev_b = *tmpb;
    }
    prev_a = *ab;
  }
}

template <typename itrT>
void convolve_point_sequence_with_polygons(polygon_set& result, itrT b, itrT e, const std::vector<polygon>& polygons) {
  using namespace boost::polygon;
  for(std::size_t i = 0; i < polygons.size(); ++i) {
    convolve_two_point_sequences(result, b, e, begin_points(polygons[i]), end_points(polygons[i]));
    for(polygon_with_holes_traits<polygon>::iterator_holes_type itrh = begin_holes(polygons[i]);
        itrh != end_holes(polygons[i]); ++itrh) {
      convolve_two_point_sequences(result, b, e, begin_points(*itrh), end_points(*itrh));
    }
  }
}

void convolve_two_polygon_sets(polygon_set& result, const polygon_set& a, const polygon_set& b) {
  using namespace boost::polygon;
  result.clear();
  std::vector<polygon> a_polygons;
  std::vector<polygon> b_polygons;
  a.get(a_polygons);
  b.get(b_polygons);
  for(std::size_t ai = 0; ai < a_polygons.size(); ++ai) {
    convolve_point_sequence_with_polygons(result, begin_points(a_polygons[ai]), 
                                          end_points(a_polygons[ai]), b_polygons);
    for(polygon_with_holes_traits<polygon>::iterator_holes_type itrh = begin_holes(a_polygons[ai]);
        itrh != end_holes(a_polygons[ai]); ++itrh) {
      convolve_point_sequence_with_polygons(result, begin_points(*itrh), 
                                            end_points(*itrh), b_polygons);
    }
    for(std::size_t bi = 0; bi < b_polygons.size(); ++bi) {
      polygon tmp_poly = a_polygons[ai];
      result.insert(convolve(tmp_poly, *(begin_points(b_polygons[bi]))));
      tmp_poly = b_polygons[bi];
      result.insert(convolve(tmp_poly, *(begin_points(a_polygons[ai]))));
    }
  }
}

double inputscale;

using namespace boost::polygon;

vector<double> Avec;
vector<double> Bvec;
vector<vector<double>> chlds;  
vector<vector<double>> retpnts;
vector<vector<double>> retholes;


void getResults(double* data, double* holesData)
{
	int index=0;
	for(int i=0;i<retpnts.size();i++){
		for(int j=0;j<retpnts[i].size();j++){
			data[index]=retpnts[i][j];			
			index++;
		}		
	}
	
	index=0;
	for(int i=0;i<retholes.size();i++){
		for(int j=0;j<retholes[i].size();j++){
			holesData[index]=retholes[i][j];
			index++;
		}		
	}	
}

void getSizes1(int* sizes)
{	
	sizes[0]=retpnts.size();
	sizes[1]=retholes.size();	
}

void getSizes2(int* sizes1,int* sizes2)
{
	for(int i=0;i<retpnts.size();i++){
		sizes1[i]=retpnts[i].size();
	}
	for(int i=0;i<retholes.size();i++){
		sizes2[i]=retholes[i].size();
	}	
}

void setData(int cntA, double* pntsA, int holesCnt, int* holesSizes, double* holesPoints, int cntB, double* pntsB) 
{
	Avec.clear();
	Bvec.clear();
	chlds.clear();
	
	retpnts.clear();
	retholes.clear();
		
	for(int i=0;i<cntA;i++){
		Avec.push_back(pntsA[i]);
	}
	for(int i=0;i<cntB;i++){
		Bvec.push_back(pntsB[i]);
	}
		
	int index=0;
	for(int i=0;i<holesCnt;i++){
		chlds.push_back(vector<double>());
		for(int j=0;j<holesSizes[i];j++){
			chlds.back().push_back(holesPoints[index]);
			index++;
		}
	}		
}


void calculateNFP() 
{
	
	polygon_set a, b, c;
	std::vector<polygon> polys;
	std::vector<point> pts;
  
  
  
  // get maximum bounds for scaling factor
  unsigned int len = Avec.size()/2;
  double Amaxx = 0;
  double Aminx = 0;
  double Amaxy = 0;
  double Aminy = 0;
  for (unsigned int i = 0; i < len; i++) {
  	double x1=Avec[i*2];
	double y1=Avec[i*2+1];
  	Amaxx = (std::max)(Amaxx, (double)x1);
  	Aminx = (std::min)(Aminx, (double)x1);
  	Amaxy = (std::max)(Amaxy, (double)y1);
  	Aminy = (std::min)(Aminy, (double)y1);
  }
  
  len = Bvec.size()/2;
  double Bmaxx = 0;
  double Bminx = 0;
  double Bmaxy = 0;
  double Bminy = 0;
  for (unsigned int i = 0; i < len*2; i+=2) {
  	double x1=Bvec[i];
	double y1=Bvec[i+1];
  	Bmaxx = (std::max)(Bmaxx, (double)x1);
  	Bminx = (std::min)(Bminx, (double)x1);
  	Bmaxy = (std::max)(Bmaxy, (double)y1);
  	Bminy = (std::min)(Bminy, (double)y1);
  }
  
  double Cmaxx = Amaxx + Bmaxx;
  double Cminx = Aminx + Bminx;
  double Cmaxy = Amaxy + Bmaxy;
  double Cminy = Aminy + Bminy;
  
  double maxxAbs = (std::max)(Cmaxx, std::fabs(Cminx));
  double maxyAbs = (std::max)(Cmaxy, std::fabs(Cminy));
  
  double maxda = (std::max)(maxxAbs, maxyAbs);
  int maxi = std::numeric_limits<int>::max();
  
  if(maxda < 1){
  	maxda = 1;
  }
  
  // why 0.1? dunno. it doesn't screw up with 0.1
  inputscale = (0.1f * (double)(maxi)) / maxda;
  
  //double scale = 1000;
  len = Avec.size()/2;
  
  for (unsigned int i = 0; i < len; i++) {
 	double x1=Avec[i*2];
	double y1=Avec[i*2+1];
    int x = (int)(inputscale * (double)x1);
    int y = (int)(inputscale * (double)y1);
        
    pts.push_back(point(x, y));
  }
  
  polygon poly;
  boost::polygon::set_points(poly, pts.begin(), pts.end());
  a+=poly;
  
  // subtract holes from a here...
  
  len = chlds.size();
  
  for(unsigned int i=0; i<len; i++){
	  vector<double> hole=chlds[i];    
    pts.clear();
    unsigned int hlen = hole.size()/2;
    for(unsigned int j=0; j<hlen; j++){
    	double x1=hole[j*2];
		double y1=hole[j*2+1];
    	int x = (int)(inputscale * (double)x1);
    	int y = (int)(inputscale * (double)y1);
    	pts.push_back(point(x, y));
    }
    boost::polygon::set_points(poly, pts.begin(), pts.end());
    a -= poly;
  }
  
  //and then load points B
  pts.clear();
  len = Bvec.size()/2;
  
  //javascript nfps are referenced with respect to the first point
  double xshift = 0;
  double yshift = 0;
  
  for (unsigned int i = 0; i < len; i++) {
	double x1=Bvec[i*2];
	double y1=Bvec[i*2+1];
    
    int x = -(int)(inputscale * (double)x1);
    int y = -(int)(inputscale * (double)y1);
    pts.push_back(point(x, y));
    
    if(i==0){
    	xshift = (double)x1;
    	yshift = (double)y1;
    }
  }
  
  boost::polygon::set_points(poly, pts.begin(), pts.end());
  b+=poly;
  
  polys.clear();
  
  convolve_two_polygon_sets(c, a, b);
  c.get(polys);
  
  for(unsigned int i = 0; i < polys.size(); ++i ){
      
  	vector<double> pointlist = vector<double>();
	
  	int j = 0;
  	  	
  	for(polygon_traits<polygon>::iterator_type itr = polys[i].begin(); itr != polys[i].end(); ++itr) {
  	   
  	 //  std::cout << (double)(*itr).get(boost::polygon::HORIZONTAL) / inputscale << std::endl;
	 double x1=((double)(*itr).get(boost::polygon::HORIZONTAL)) / inputscale + xshift;
	 double y1=((double)(*itr).get(boost::polygon::VERTICAL)) / inputscale + yshift;
       
	   pointlist.push_back(x1);	   
	   pointlist.push_back(y1);	 
       
       j++;
    }
    retpnts.push_back(pointlist);
    // holes
    
    int k = 0;
    for(polygon_with_holes_traits<polygon>::iterator_holes_type itrh = begin_holes(polys[i]); itrh != end_holes(polys[i]); ++itrh){
		vector<double> child=vector<double>();
		
    	
    	int z = 0;
    	for(polygon_traits<polygon>::iterator_type itr2 = (*itrh).begin(); itr2 != (*itrh).end(); ++itr2) {
    		
			double x1=((double)(*itr2).get(boost::polygon::HORIZONTAL)) / inputscale + xshift;
			double y1=((double)(*itr2).get(boost::polygon::VERTICAL)) / inputscale + yshift;
    		
    		child.push_back(x1);
			child.push_back(y1);
    		
    		z++;
    	}
		retholes.push_back(child);
    	
    	k++;
    }    
  }
}