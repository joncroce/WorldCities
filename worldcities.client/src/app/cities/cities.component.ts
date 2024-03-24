import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import type { City } from './city';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.scss']
})
export class CitiesComponent implements OnInit {
  public displayedColumns: Array<string> = [
    'id', 'name', 'lat', 'lon'
  ];
  public cities!: MatTableDataSource<City>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    var apiEndpoint = environment.baseUrl + 'api/Cities';
    this.http.get<Array<City>>(apiEndpoint).subscribe({
      next: (result) => {
        this.cities = new MatTableDataSource<City>(result);
        this.cities.paginator = this.paginator;
      },
      error: (error) => console.error(error)
    });
  }
}
