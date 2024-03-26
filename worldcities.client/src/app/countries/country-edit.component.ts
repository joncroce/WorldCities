import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { AbstractControl, AsyncValidatorFn, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map } from 'rxjs';
import type { Country } from './country';

@Component({
  selector: 'app-country-edit',
  templateUrl: './country-edit.component.html',
  styleUrls: ['./country-edit.component.scss']
})
export class CountryEditComponent implements OnInit {
  getErrors(
    control: AbstractControl,
    displayName: string,
  ): Array<String> {
    var errors: Array<string> = [];

    Object.keys(control.errors ?? {}).forEach((key) => {
      switch (key) {
        case 'required':
          errors.push(`${displayName} is required.`);
          break;
        case 'pattern':
          errors.push(`${displayName} contains invalid characters.`);
          break;
        case 'isDupeField':
          errors.push(`${displayName} already exists; please choose another.`);
          break;
        default:
          errors.push(`${displayName} is invalid.`);
      }
    });

    return errors;
  }

  title?: string;
  form!: FormGroup;
  country?: Country;
  // The country object id as fetched from the active route
  // NULL when adding a new country, and
  // NOT NULL when editing an existing country.
  id?: number;
  // The countries array for the select
  countries?: Array<Country>;

  constructor(
    private fb: FormBuilder,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) { }

  ngOnInit() {
    this.form = this.fb.group({
      name: ['',
        Validators.required,
        this.isDupeField("name")
      ],
      iso2: ['',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-z]{2}$/)
        ],
        this.isDupeField("iso2")
      ],
      iso3: ['',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-z]{3}$/)
        ],
        this.isDupeField("iso3")
      ]
    });

    this.loadData();
  }

  loadData() {
    var idParam = this.activatedRoute.snapshot.paramMap.get("id");
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // EDIT mode
      var url = environment.baseUrl + "api/Countries/" + this.id;
      this.http.get<Country>(url).subscribe({
        next: (result) => {
          this.country = result;
          this.title = "Edit - " + this.country.name;

          this.form.patchValue(this.country);
        },
        error: (error) => console.error(error)
      });
    } else {
      // CREATE mode
      this.title = "Create a new Country";
    }
  }

  onSubmit() {
    var country = (this.id) ? this.country : <Country>{};
    if (country) {
      country.name = this.form.controls["name"].value;
      country.iso2 = this.form.controls["iso2"].value;
      country.iso3 = this.form.controls["iso3"].value;

      if (this.id) {
        // EDIT mode
        var url = environment.baseUrl + "api/Countries/" + country.id;
        this.http.put<Country>(url, country).subscribe({
          next: (result) => {
            console.log("Country " + country!.id + " has been updated.");
            this.router.navigate(['/countries']);
          },
          error: (error) => console.error(error)
        });
      } else {
        // CREATE mode
        var url = environment.baseUrl + "api/Countries";
        this.http.post<Country>(url, country).subscribe({
          next: (result) => {
            console.log("Country " + result.id + " has been created.");
            this.router.navigate(["/countries"]);
          },
          error: (error) => console.error(error)
        });
      }
    }
  }

  isDupeField(fieldName: string): AsyncValidatorFn {
    return (control: AbstractControl): Observable<{
      [key: string]: any
    } | null> => {
      var params = new HttpParams()
        .set("countryId", (this.id) ? this.id.toString() : "0")
        .set("fieldName", fieldName)
        .set("fieldValue", control.value);
      var url = environment.baseUrl + "api/Countries/IsDupeField";

      return this.http.post<boolean>(url, null, { params })
        .pipe(map(result => (result ? { isDupeField: true } : null)));
    }
  }
}
