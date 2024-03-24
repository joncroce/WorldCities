import { Component, OnInit } from '@angular/core';
import { City } from './city';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.scss']
})
export class CitiesComponent implements OnInit {
  public displayedColumns: Array<string> = [
    'id', 'name', 'lat', 'lon'
  ];
  public cities!: Array<City>;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    var apiEndpoint = environment.baseUrl + 'api/Cities';
    this.http.get<Array<City>>(apiEndpoint).subscribe({
      next: (result) => { this.cities = result },
      error: (error) => console.error(error)
    });
  }
}
