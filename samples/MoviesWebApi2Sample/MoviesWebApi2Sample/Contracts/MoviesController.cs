// Template: Base Controller (ApiControllerBase.t4) version 3.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MoviesWebApi2Sample.Movies.Models;

// Do not modify this file. This code was generated by RAML Web Api 2 Scaffolder

namespace MoviesWebApi2Sample.Movies
{
    [RoutePrefix("movies")]
    public partial class MoviesController : ApiController
    {


        /// <summary>
		/// gets all movies in the catalogue
		/// </summary>
		/// <returns>IList&lt;MoviesGetOKResponseContent&gt;</returns>
        [ResponseType(typeof(IList<MoviesGetOKResponseContent>))]
        [HttpGet]
        [Route("")]
        public virtual IHttpActionResult GetBase()
        {
            // Do not modify this code
            return  ((IMoviesController)this).Get();
        }

        /// <summary>
		/// adds a movie to the catalogue
		/// </summary>
		/// <param name="moviespostrequestcontent"></param>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
        [HttpPost]
        [Route("")]
        public virtual IHttpActionResult PostBase(Models.MoviesPostRequestContent moviespostrequestcontent,[FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).Post(moviespostrequestcontent,access_token);
        }

        /// <summary>
		/// get the info of a movie
		/// </summary>
		/// <param name="id"></param>
		/// <returns>MoviesIdGetOKResponseContent</returns>
        [ResponseType(typeof(MoviesIdGetOKResponseContent))]
        [HttpGet]
        [Route("{id}")]
        public virtual IHttpActionResult GetByIdBase([FromUri] string id)
        {
            // Do not modify this code
            return  ((IMoviesController)this).GetById(id);
        }

        /// <summary>
		/// update the info of a movie
		/// </summary>
		/// <param name="moviesidputrequestcontent"></param>
		/// <param name="id"></param>
        [HttpPut]
        [Route("{id}")]
        public virtual IHttpActionResult PutBase(Models.MoviesIdPutRequestContent moviesidputrequestcontent,[FromUri] string id)
        {
            // Do not modify this code
            return  ((IMoviesController)this).Put(moviesidputrequestcontent,id);
        }

        /// <summary>
		/// remove a movie from the catalogue
		/// </summary>
		/// <param name="id"></param>
        [HttpDelete]
        [Route("{id}")]
        public virtual IHttpActionResult DeleteBase([FromUri] string id)
        {
            // Do not modify this code
            return  ((IMoviesController)this).Delete(id);
        }

        /// <summary>
		/// rent a movie
		/// </summary>
		/// <param name="content"></param>
		/// <param name="id"></param>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
        [HttpPut]
        [Route("{id}/rent")]
        public virtual IHttpActionResult PutRentBase([FromBody] string content,[FromUri] string id,[FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).PutRent(content,id,access_token);
        }

        /// <summary>
		/// return a movie
		/// </summary>
		/// <param name="content"></param>
		/// <param name="id"></param>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
        [HttpPut]
        [Route("{id}/return")]
        public virtual IHttpActionResult PutReturnBase([FromBody] string content,[FromUri] string id,[FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).PutReturn(content,id,access_token);
        }

        /// <summary>
		/// gets the current user movies wishlist
		/// </summary>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
		/// <returns>IList&lt;MoviesWishlistGetOKResponseContent&gt;</returns>
        [ResponseType(typeof(IList<MoviesWishlistGetOKResponseContent>))]
        [HttpGet]
        [Route("wishlist")]
        public virtual IHttpActionResult GetWishlistBase([FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).GetWishlist(access_token);
        }

        /// <summary>
		/// add a movie to the current user movies wishlist
		/// </summary>
		/// <param name="content"></param>
		/// <param name="id"></param>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
        [HttpPost]
        [Route("wishlist/{id}")]
        public virtual IHttpActionResult PostByIdBase([FromBody] string content,[FromUri] string id,[FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).PostById(content,id,access_token);
        }

        /// <summary>
		/// removes a movie from the current user movies wishlist
		/// </summary>
		/// <param name="id"></param>
		/// <param name="access_token">Used to send a valid OAuth 2 access token. Do not use together with the &quot;Authorization&quot; header </param>
        [HttpDelete]
        [Route("wishlist/{id}")]
        public virtual IHttpActionResult DeleteByIdBase([FromUri] string id,[FromUri] string access_token = null)
        {
            // Do not modify this code
            return  ((IMoviesController)this).DeleteById(id,access_token);
        }

        /// <summary>
		/// gets the user rented movies
		/// </summary>
		/// <returns>IList&lt;MoviesRentedGetOKResponseContent&gt;</returns>
        [ResponseType(typeof(IList<MoviesRentedGetOKResponseContent>))]
        [HttpGet]
        [Route("rented")]
        public virtual IHttpActionResult GetRentedBase()
        {
            // Do not modify this code
            return  ((IMoviesController)this).GetRented();
        }

        /// <summary>
		/// get all movies that are not currently rented
		/// </summary>
		/// <returns>IList&lt;MoviesAvailableGetOKResponseContent&gt;</returns>
        [ResponseType(typeof(IList<MoviesAvailableGetOKResponseContent>))]
        [HttpGet]
        [Route("available")]
        public virtual IHttpActionResult GetAvailableBase()
        {
            // Do not modify this code
            return  ((IMoviesController)this).GetAvailable();
        }
    }
}
