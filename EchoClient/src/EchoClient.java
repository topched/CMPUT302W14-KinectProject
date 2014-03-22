
/*************************************************************************
 *  Compilation:  javac EchoClient.java In.java Out.java
 *  Execution:    java EchoClient name host
 *
 *  Connects to host server on port 4444, sends text, and prints out
 *  whatever the server sends back.
 *
 *  
 *  % java EchoClient wayne localhost
 *  Connected to localhost on port 4444
 *  this is a test
 *  [wayne]: this is a test
 *  it works
 *  [wayne]: it works
 *  <Ctrl-d>                 
 *  Closing connection to localhost
 *
 *  Windows users: replace <Ctrl-d> with <Ctrl-z>
 *  
 *************************************************************************/

import java.net.Socket;


public class EchoClient {

    public static void main(String[] args) throws Exception {
       // String screenName = args[0];
        String host       = args[0];
        int port          = 4444;

        // connect to server and open up IO streams
        Socket socket = new Socket(host, port);
        In     stdin  = new In();
        In     in     = new In(socket);
        Out    out    = new Out(socket);
        System.err.println("Connected to " + host + " on port " + port);
        
        
        // read in a line from stdin, send to server, and print back reply
        while (stdin.hasNextLine()) {

            // read line of client
            String s = stdin.readLine();

            // send over socket to server
           // out.println("[" + screenName + "]: " + s);
            out.println(s);
            
            // get reply from server and print it out
            System.out.println(in.readLine());
            
            if(s.contentEquals("Quit")){
                // close IO streams, then socket
                System.err.println("Closing connection to " + host);
                out.close();
                in.close();
                socket.close();
            }
        }

        
        // close IO streams, then socket
        System.err.println("Closing connection to " + host);
        out.close();
        in.close();
        socket.close();
    }
    // listen to socket and print everything that server broadcasts
    public void listen() {
        String s;
        while ((s = in.readLine()) != null) {
            
        }
        out.close();
        in.close();
        try                 { socket.close();      }
        catch (Exception e) { e.printStackTrace(); }
        System.err.println("Closed client socket");
    }
}