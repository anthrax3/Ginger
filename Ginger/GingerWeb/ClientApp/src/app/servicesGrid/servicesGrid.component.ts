import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'services-grid-data',
  templateUrl: './servicesgrid.component.html'
})


export class ServicesGridComponent
{
  public forecasts: WeatherForecast[];
  public report: string;
  mHttp: HttpClient;
  mBaseUrl: string;

  
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string)
  {
    this.mHttp = http;
    this.mBaseUrl = baseUrl;

    http.get<WeatherForecast[]>(baseUrl + 'api/ServiceGrid/NodeList').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));

  }


}


interface WeatherForecast {
  name: string;
  description: string;
  fileName: string;
  status: string;
  elapsed: number;  
}
